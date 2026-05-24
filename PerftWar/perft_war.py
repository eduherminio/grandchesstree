#!/usr/bin/env python3
"""Perft-War benchmark harness for the TGCT leaderboard.

See README.md for the design. Stdlib-only by design — this is meant to run
on a fresh baremetal Ubuntu host without extra dependencies.
"""

from __future__ import annotations

import argparse
import json
import os
import queue
import re
import statistics
import subprocess
import sys
import threading
import time
from datetime import datetime, timezone
from pathlib import Path

MODES = ("single-no-cache", "single-with-cache", "multi-no-cache", "multi-with-cache")

# (name, fen, {depth: expected_nodes})
# Authoritative node counts from results/perft_p{0,1,2}_results.json.
# The runner walks depths in order (d1, d2, …) under a per-position time budget
# and stops when the budget is exhausted; the deepest completed depth is what
# the leaderboard reports for that position.
POSITIONS = [
    (
        "startpos",
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
        {
            1: 20,
            2: 400,
            3: 8_902,
            4: 197_281,
            5: 4_865_609,
            6: 119_060_324,
            7: 3_195_901_860,
            8: 84_998_978_956,
            9: 2_439_530_234_167,
            10: 69_352_859_712_417,
            11: 2_097_651_003_696_806,
            12: 62_854_969_236_701_747,
        },
    ),
    (
        "kiwipete",
        "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
        {
            1: 48,
            2: 2_039,
            3: 97_862,
            4: 4_085_603,
            5: 193_690_690,
            6: 8_031_647_685,
            7: 374_190_009_323,
            8: 15_493_944_087_984,
            9: 708_027_759_953_502,
        },
    ),
    (
        "sje",
        "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10",
        {
            1: 46,
            2: 2_079,
            3: 89_890,
            4: 3_894_594,
            5: 164_075_551,
            6: 6_923_051_137,
            7: 287_188_994_746,
            8: 11_923_589_843_526,
            9: 490_154_852_788_714,
        },
    ),
]


def utc_now_iso() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def load_descriptor(path: Path) -> dict:
    desc = json.loads(path.read_text())
    for key in ("name", "version", "modes"):
        if key not in desc:
            raise ValueError(f"descriptor {path} missing required key: {key}")
    for mode_name, mode_def in desc["modes"].items():
        if mode_name not in MODES:
            raise ValueError(f"descriptor {path} declares unknown mode: {mode_name}")
        if not isinstance(mode_def, dict):
            raise ValueError(f"mode {mode_name} must be an object")
        for req in ("launch", "case", "end_re"):
            if req not in mode_def:
                raise ValueError(f"mode {mode_name} missing required key: {req}")
    return desc


def substitute(template: str, fen: str, depth: int, threads: int) -> str:
    return (template
            .replace("{fen}", fen)
            .replace("{depth}", str(depth))
            .replace("{threads}", str(threads)))


def host_threads() -> int:
    return os.cpu_count() or 1


def verify_nodes(stdout: str, expected: int) -> bool:
    return re.search(rf"\b{expected}\b", stdout) is not None


def stdout_tail(stdout: str, lines: int = 20) -> str:
    return "\n".join(stdout.splitlines()[-lines:])


class Disqualified(Exception):
    def __init__(self, mode: str, case: dict, stdout: str):
        self.mode = mode
        self.case = case
        self.stdout = stdout


def _start_reader(proc: subprocess.Popen, q: "queue.Queue[str | None]") -> threading.Thread:
    def reader() -> None:
        try:
            assert proc.stdout is not None
            for line in proc.stdout:
                q.put(line)
        finally:
            q.put(None)
    t = threading.Thread(target=reader, daemon=True)
    t.start()
    return t


def _send_lines(proc: subprocess.Popen, text: str) -> None:
    assert proc.stdin is not None
    for line in text.split("\n"):
        if not line:
            continue
        proc.stdin.write(line + "\n")
    proc.stdin.flush()


def _wait_for_end(
    q: "queue.Queue[str | None]",
    end_re: re.Pattern,
    timeout: float,
) -> tuple[str, bool, bool]:
    """Read lines until end_re matches or timeout. Returns (output, timed_out, eof)."""
    deadline = time.monotonic() + timeout
    lines: list[str] = []
    while True:
        remaining = deadline - time.monotonic()
        if remaining <= 0:
            return "\n".join(lines), True, False
        try:
            line = q.get(timeout=remaining)
        except queue.Empty:
            return "\n".join(lines), True, False
        if line is None:
            return "\n".join(lines), False, True
        stripped = line.rstrip("\n")
        lines.append(stripped)
        if end_re.search(stripped):
            return "\n".join(lines), False, False


def _parse_cputime(s: str) -> float | None:
    """Parse a `ps` time-column value into seconds.

    macOS BSD ps:  '[H:]MM:SS.dd'  (e.g. '0:01.23', '1:23:45.67')
    Linux GNU ps:  '[DD-]HH:MM:SS' (e.g. '00:01:23', '1-02:30:45')
    """
    try:
        s = s.strip()
        if "-" in s:
            days_str, rest = s.split("-", 1)
            days_sec = int(days_str) * 86_400
        else:
            days_sec = 0
            rest = s
        sec = 0.0
        for p in rest.split(":"):
            sec = sec * 60 + float(p)
        return days_sec + sec
    except (ValueError, AttributeError):
        return None


def _ps_snapshot(pid: int) -> dict | None:
    """One-shot {rss_kb, cputime_sec} read via `ps`. Returns None on Windows
    (no `ps`) or any error — process monitoring is best-effort."""
    try:
        result = subprocess.run(
            ["ps", "-p", str(pid), "-o", "rss=,time="],
            capture_output=True, text=True, timeout=2,
        )
    except (FileNotFoundError, subprocess.TimeoutExpired, OSError):
        return None
    if result.returncode != 0:
        return None
    line = result.stdout.strip()
    if not line:
        return None
    parts = line.split(None, 1)
    if len(parts) < 2:
        return None
    try:
        rss_kb = int(parts[0])
    except ValueError:
        return None
    cputime_sec = _parse_cputime(parts[1])
    if cputime_sec is None:
        return None
    return {"rss_kb": rss_kb, "cputime_sec": cputime_sec}


class ProcStatsSampler:
    """Background-thread RSS sampler plus synchronous start/end snapshots.

    av_cpu_pct is derived from cputime_delta / wall_elapsed * 100, so it can
    exceed 100% on multi-threaded workloads (e.g. 14 cores pinned = ~1400%).
    """

    def __init__(self, pid: int, interval_sec: float = 0.1):
        self.pid = pid
        self.interval = interval_sec
        self._lock = threading.Lock()
        self._samples: list[int] = []
        self._stop = threading.Event()
        self._thread = threading.Thread(target=self._run, daemon=True)
        self.available = _ps_snapshot(pid) is not None

    def start(self) -> None:
        if self.available:
            self._thread.start()

    def stop(self) -> None:
        self._stop.set()
        if self._thread.is_alive():
            self._thread.join(timeout=2)

    def _run(self) -> None:
        while not self._stop.is_set():
            snap = _ps_snapshot(self.pid)
            if snap:
                with self._lock:
                    self._samples.append(snap["rss_kb"])
            if self._stop.wait(self.interval):
                break

    def begin_call(self) -> tuple[dict | None, int]:
        if not self.available:
            return None, 0
        snap = _ps_snapshot(self.pid)
        with self._lock:
            start_idx = len(self._samples)
        return snap, start_idx

    def end_call(self, marker: tuple[dict | None, int], elapsed_sec: float) -> dict:
        if not self.available:
            return {}
        start_snap, start_idx = marker
        end_snap = _ps_snapshot(self.pid)
        with self._lock:
            samples = list(self._samples[start_idx:])

        result: dict = {}
        if start_snap and end_snap and elapsed_sec > 0:
            cpu_delta = end_snap["cputime_sec"] - start_snap["cputime_sec"]
            result["av_cpu_pct"] = round(cpu_delta / elapsed_sec * 100, 1)

        rss_pool = list(samples)
        if start_snap:
            rss_pool.append(start_snap["rss_kb"])
        if end_snap:
            rss_pool.append(end_snap["rss_kb"])
        if rss_pool:
            result["av_rss_mb"] = round(sum(rss_pool) / len(rss_pool) / 1024, 1)
            result["peak_rss_mb"] = round(max(rss_pool) / 1024, 1)

        return result


def _shutdown_proc(proc: subprocess.Popen, quit_cmd: str | None) -> None:
    try:
        if quit_cmd and proc.stdin and not proc.stdin.closed:
            try:
                proc.stdin.write(quit_cmd + "\n")
                proc.stdin.flush()
            except (BrokenPipeError, OSError):
                pass
        if proc.stdin:
            try:
                proc.stdin.close()
            except (BrokenPipeError, OSError):
                pass
        try:
            proc.wait(timeout=5)
        except subprocess.TimeoutExpired:
            proc.kill()
            proc.wait()
    except Exception:
        pass


def run_position_iterative(
    mode: str,
    mode_def: dict,
    name: str,
    fen: str,
    depths_map: dict[int, int],
    budget_sec: float,
    threads: int,
) -> dict:
    """Iterative deepening on one position under a hard wall-clock budget.

    Launches a fresh engine subprocess, sends d1, d2, …, recording the time
    each completed depth took. When `budget_sec` elapses or the current call
    times out, the subprocess is killed and the deepest completed depth is
    used as the position's result."""
    launch = substitute(mode_def["launch"], "", 0, threads)
    setup_lines = [substitute(s, "", 0, threads) for s in mode_def.get("setup", [])]
    case_template = mode_def["case"]
    end_re = re.compile(mode_def["end_re"])
    quit_cmd = mode_def.get("quit", "quit")

    # `exec` so the shell replaces itself with the engine; proc.pid then refers
    # to the engine itself, which the stats sampler needs for accurate readings.
    proc = subprocess.Popen(
        f"exec {launch}",
        shell=True,
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        bufsize=1,
    )
    q: "queue.Queue[str | None]" = queue.Queue()
    _start_reader(proc, q)
    for line in setup_lines:
        _send_lines(proc, line)

    sampler = ProcStatsSampler(proc.pid)
    sampler.start()

    completed: list[dict] = []
    note: str | None = None
    wrong_count_case: dict | None = None
    budget_start = time.monotonic()

    try:
        for depth in sorted(depths_map):
            elapsed_total = time.monotonic() - budget_start
            remaining = budget_sec - elapsed_total
            if remaining <= 0:
                note = "budget exhausted"
                break

            cmd = substitute(case_template, fen, depth, threads)
            stats_marker = sampler.begin_call()
            call_start = time.monotonic()
            _send_lines(proc, cmd)
            output, timed_out, eof = _wait_for_end(q, end_re, remaining)
            call_elapsed = time.monotonic() - call_start
            stats = sampler.end_call(stats_marker, call_elapsed)

            if timed_out:
                print(f"    d{depth:<2} TIMEOUT after {call_elapsed:.1f}s (budget exhausted)", flush=True)
                note = f"timeout at d{depth}"
                break
            if eof:
                print(f"    d{depth:<2} ENGINE DIED after {call_elapsed:.1f}s", flush=True)
                note = f"engine died at d{depth}"
                break

            expected = depths_map[depth]
            if not verify_nodes(output, expected):
                print(f"    d{depth:<2} WRONG NODE COUNT", flush=True)
                wrong_count_case = {
                    "name": name, "fen": fen, "depth": depth,
                    "expected_nodes": expected,
                }
                note = f"wrong node count at d{depth}"
                # Bubble up via exception after cleanup.
                raise Disqualified(mode, wrong_count_case, output)

            nps = int(expected / call_elapsed) if call_elapsed > 0 else 0
            entry = {
                "depth": depth,
                "nodes": expected,
                "elapsed_sec": round(call_elapsed, 6),
                "nps": nps,
                **stats,
            }
            completed.append(entry)
            cpu_str = f"{stats['av_cpu_pct']:>6.0f}%" if "av_cpu_pct" in stats else "    -- "
            rss_str = f"{stats['peak_rss_mb']:>7.0f}MB" if "peak_rss_mb" in stats else "      --"
            print(f"    d{depth:<2} {call_elapsed:>8.3f}s  {expected:>18,} nodes  {nps:>14,} nps  cpu={cpu_str}  rss={rss_str}", flush=True)
    finally:
        sampler.stop()
        _shutdown_proc(proc, quit_cmd)

    result: dict = {"name": name, "fen": fen, "depths": completed}
    if note:
        result["note"] = note
    if completed:
        deepest = completed[-1]
        result["best_depth"] = deepest["depth"]
        result["best_nodes"] = deepest["nodes"]
        result["best_elapsed_sec"] = deepest["elapsed_sec"]
        result["best_nps"] = deepest["nps"]
        for k in ("av_cpu_pct", "av_rss_mb", "peak_rss_mb"):
            if k in deepest:
                result[f"best_{k}"] = deepest[k]
    return result


def run_mode(
    mode: str,
    mode_def: dict,
    positions: list[tuple[str, str, dict[int, int]]],
    budget_sec: float,
    threads: int,
) -> dict:
    position_results: list[dict] = []
    best_nps_values: list[float] = []
    for name, fen, depths_map in positions:
        print(f"  -- {name} (budget {budget_sec:.0f}s) --", flush=True)
        result = run_position_iterative(mode, mode_def, name, fen, depths_map, budget_sec, threads)
        position_results.append(result)
        if "best_nps" in result and result["best_nps"] is not None:
            best_nps_values.append(result["best_nps"])

    mean_nps = int(statistics.mean(best_nps_values)) if best_nps_values else None
    return {"positions": position_results, "mean_nps": mean_nps}


def write_engine_result(results_dir: Path, descriptor: dict, version_block: dict) -> Path:
    results_dir.mkdir(parents=True, exist_ok=True)
    path = results_dir / f"{descriptor['name']}.json"

    if path.exists():
        existing = json.loads(path.read_text())
    else:
        existing = {
            "name": descriptor["name"],
            "owner": descriptor.get("owner"),
            "repo": descriptor.get("repo"),
            "versions": {},
        }

    existing["owner"] = descriptor.get("owner", existing.get("owner"))
    existing["repo"] = descriptor.get("repo", existing.get("repo"))
    existing.setdefault("versions", {})
    existing["versions"][descriptor["version"]] = version_block

    path.write_text(json.dumps(existing, indent=2) + "\n")
    return path


def cmd_run(args: argparse.Namespace) -> int:
    descriptor = load_descriptor(Path(args.descriptor))

    requested_modes = args.modes.split(",") if args.modes else list(descriptor["modes"].keys())
    for mode in requested_modes:
        if mode not in MODES:
            print(f"unknown mode: {mode}", file=sys.stderr)
            return 2
        if mode not in descriptor["modes"]:
            print(f"engine {descriptor['name']} does not declare mode {mode}; skipping",
                  file=sys.stderr)

    threads = host_threads()
    budget = float(args.budget_sec)

    if args.positions:
        wanted = set(args.positions.split(","))
        positions = [p for p in POSITIONS if p[0] in wanted]
        unknown = wanted - {p[0] for p in POSITIONS}
        if unknown:
            print(f"unknown positions: {sorted(unknown)}", file=sys.stderr)
            return 2
    else:
        positions = list(POSITIONS)

    mode_results: dict[str, dict] = {}
    try:
        for mode in requested_modes:
            if mode not in descriptor["modes"]:
                continue
            print(f"[{descriptor['name']} {descriptor['version']}] {mode} (threads={threads})", flush=True)
            mode_results[mode] = run_mode(
                mode=mode,
                mode_def=descriptor["modes"][mode],
                positions=positions,
                budget_sec=budget,
                threads=threads,
            )
    except Disqualified as dq:
        version_block = {
            "ran_at": utc_now_iso(),
            "disqualified": True,
            "reason": "wrong node count",
            "failed_case": {
                "mode": dq.mode,
                "name": dq.case["name"],
                "fen": dq.case["fen"],
                "depth": dq.case["depth"],
                "expected_nodes": dq.case["expected_nodes"],
                "captured_stdout": stdout_tail(dq.stdout, 20),
            },
        }
        path = write_engine_result(Path(args.results_dir), descriptor, version_block)
        print(f"DISQUALIFIED: {dq.mode} {dq.case['name']} d{dq.case['depth']}",
              file=sys.stderr)
        print(f"wrote {path}", file=sys.stderr)
        return 1

    version_block = {
        "ran_at": utc_now_iso(),
        "disqualified": False,
        "budget_sec": budget,
        "modes": mode_results,
    }
    path = write_engine_result(Path(args.results_dir), descriptor, version_block)
    print(f"wrote {path}")
    return 0


def cmd_aggregate(args: argparse.Namespace) -> int:
    results_dir = Path(args.results_dir)
    rows: list[dict] = []
    for engine_file in sorted(results_dir.glob("*.json")):
        if engine_file.name == "leaderboard.json":
            continue
        data = json.loads(engine_file.read_text())
        versions = data.get("versions", {})
        valid = [
            (v, block) for v, block in versions.items()
            if not block.get("disqualified", False) and "modes" in block
        ]
        if not valid:
            continue
        valid.sort(key=lambda vb: vb[1].get("ran_at", ""), reverse=True)
        version, block = valid[0]
        for mode, mode_data in block["modes"].items():
            row_positions = []
            for pos in mode_data.get("positions", []):
                row_positions.append({
                    "name": pos["name"],
                    "best_depth": pos.get("best_depth"),
                    "best_nodes": pos.get("best_nodes"),
                    "best_elapsed_sec": pos.get("best_elapsed_sec"),
                    "best_nps": pos.get("best_nps"),
                    "best_av_cpu_pct": pos.get("best_av_cpu_pct"),
                    "best_av_rss_mb": pos.get("best_av_rss_mb"),
                    "best_peak_rss_mb": pos.get("best_peak_rss_mb"),
                    "depths": [
                        {
                            "depth": d["depth"],
                            "elapsed_sec": d["elapsed_sec"],
                            "nps": d.get("nps"),
                            "av_cpu_pct": d.get("av_cpu_pct"),
                            "av_rss_mb": d.get("av_rss_mb"),
                            "peak_rss_mb": d.get("peak_rss_mb"),
                        }
                        for d in pos.get("depths", [])
                    ],
                })
            rows.append({
                "engine": data["name"],
                "version": version,
                "mode": mode,
                "mean_nps": mode_data.get("mean_nps"),
                "positions": row_positions,
            })

    out_path = Path(args.out)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(json.dumps({"generated_at": utc_now_iso(), "rows": rows}, indent=2) + "\n")
    print(f"wrote {out_path} ({len(rows)} rows)")
    return 0


def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(prog="perft_war.py")
    sub = p.add_subparsers(dest="cmd", required=True)

    r = sub.add_parser("run", help="benchmark one engine")
    r.add_argument("descriptor", help="path to engine descriptor JSON")
    r.add_argument("--budget-sec", type=float, default=60.0,
                   help="wall-clock budget per (mode, position) pair in seconds")
    r.add_argument("--modes", default=None,
                   help="comma-separated subset of modes; default = all declared")
    r.add_argument("--positions", default=None,
                   help="comma-separated subset of positions; default = all in POSITIONS")
    r.add_argument("--results-dir", default="results")
    r.set_defaults(func=cmd_run)

    a = sub.add_parser("aggregate", help="build the combined leaderboard")
    a.add_argument("--results-dir", default="results")
    a.add_argument("--out", default="results/leaderboard.json")
    a.set_defaults(func=cmd_aggregate)

    return p


def main(argv: list[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    return args.func(args)


if __name__ == "__main__":
    raise SystemExit(main())
