# Perft-War

Benchmark harness for the TGCT leaderboard. Given an engine, measures perft
NPS (nodes/sec) under four conditions (single/multi × no-cache/with-cache),
strictly verifies correctness, and writes the results to a per-engine JSON
file that can later be rolled up into a combined leaderboard.

**Status:** plan only. No code yet. This document is the working agreement —
when we pick this up in another session, implement against it.

---

## Goal

A Python script (`perft_war.py`) that ingests an engine descriptor (JSON) and
runs a fixed benchmark suite against the engine binary. Designed to be invoked
on an isolated baremetal Ubuntu host — no sandboxing in the harness itself.

**Correctness first.** Any wrong node count anywhere → the engine run is
disqualified. The leaderboard is for engines that already pass; speed is the
tiebreaker among correct ones.

## The four modes

Every engine declares which modes it supports. Each mode is a separate run.

| Mode                | Threads        | Cache (TT) |
|---------------------|----------------|------------|
| `single-no-cache`   | 1              | none / 0   |
| `single-with-cache` | 1              | 4 GB       |
| `multi-no-cache`    | all host cores | none / 0   |
| `multi-with-cache`  | all host cores | 4 GB       |

The engine maintainer's `command` for each mode encodes those settings
however the engine wants them (CLI flag, env var, UCI setoption inside a
wrapper script, etc.). The harness doesn't care.

## Engine descriptor

One JSON file per engine, e.g. `engines/stockfish.json`. Schema:

```json
{
  "name":    "stockfish",
  "version": "17",
  "owner":   "official-stockfish",
  "repo":    "https://github.com/official-stockfish/Stockfish",
  "modes": {
    "single-no-cache":   "stockfish-bench --threads 1        --hash 0    --fen \"{fen}\" --depth {depth}",
    "single-with-cache": "stockfish-bench --threads 1        --hash 4096 --fen \"{fen}\" --depth {depth}",
    "multi-no-cache":    "stockfish-bench --threads $(nproc) --hash 0    --fen \"{fen}\" --depth {depth}",
    "multi-with-cache":  "stockfish-bench --threads $(nproc) --hash 4096 --fen \"{fen}\" --depth {depth}"
  }
}
```

Notes:

- Each mode value is a **single shell command string** with `{fen}` and
  `{depth}` placeholders. Invoked via `subprocess.run(shell=True)` so shell
  expansion (`$(nproc)`, pipes, env) works.
- Engines opt in only to the modes they support — omit any mode they don't.
- Output (stdout) must contain the perft node count somewhere as a plain
  integer. That's all the harness reads.

## Position set

Six TGCT-canonical positions × two depths (4 and 6) = **12 cases per mode**.

Reference node counts below are needed for the strict-verify check.
**Status legend:** ✅ verified against bundled `perft.epd`; 🟡 computed via
`MoveGen.App` (which itself passed 125k cases against `perftcheck`), but
should be cross-checked against Stockfish or similar before committing;
❓ unconfirmed.

| # | Name      | FEN                                                                                          | D4              | D6                |
|---|-----------|----------------------------------------------------------------------------------------------|-----------------|-------------------|
| 1 | startpos  | `rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1`                                   | 197,281 ✅      | 119,060,324 ✅    |
| 2 | kiwipete  | `r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1`                       | 4,085,603 ✅    | 8,031,647,685 ✅  |
| 3 | sje       | `r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10`                   | 3,894,594 ✅    | 6,923,051,137 ✅  |
| 4 | pos3      | `8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1`                                                  | 43,238 🟡       | 11,030,083 ✅     |
| 5 | pos4      | `r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1` (note: see below)         | 422,333 🟡      | 706,045,033 🟡    |
| 6 | pos5      | `rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8`                                  | 2,103,487 ✅    | 3,048,196,529 🟡  |

Sources used so far:
- ✅ values come from `PerftSuite/data/perft.epd` or `results/perft_p*_d*_total.json`.
- 🟡 values come from running `MoveGen/MoveGen.App` directly.

**Open: Pos4 FEN.** The codebase uses `Pp1P2PP` (white pawns on g2 and h2) on
rank 2, but the canonical CPW Position 4 uses `Pp1P2pP` (black pawn on g2,
white on h2). MoveGen.App's reported D4 and D6 for the `Pp1P2PP` variant
happened to equal the canonical counts, which deserves a sanity check before
the harness locks in these expected values. Decide which variant to keep,
then re-derive D4/D6 if changing.

## Execution model

For each (engine, mode) tuple:

1. **Warmup pass** — run all 12 cases once in randomized order, throw the
   timings away. Mostly relevant for JIT/cold-cache runtimes (.NET, JVM,
   Node, WASM).
2. **Measured passes** — `--reps 5` (default). For each rep, generate a new
   permutation of the 12 cases under the constraint that **no two consecutive
   runs share the same (position, depth)**. Avoids any one case benefitting
   from a sibling's hot CPU caches / paged-in data.
3. Per case, take the **median NPS** across the 5 measured runs.
4. Per mode, take the **mean of medians** across the 12 cases → headline
   `mean_nps`.

Each rep is a fresh subprocess (because cache mode is set at process spawn
via the command, not state). 1 subprocess = 1 (FEN, depth) measurement.

## Verification

- After each subprocess exits, scan stdout for `\b<expected_nodes>\b` (regex,
  word boundaries to avoid substring false matches).
- **Mismatch** → fail-fast: stop the entire engine run, write the version
  block as `{ "disqualified": true, "reason": "wrong node count",
  "failed_case": { fen, depth, expected, captured_stdout } }`. No NPS values
  are written.
- **Timeout (>10 min/run)** → drop that single rep, don't disqualify. Logged
  with `runs[].timeout: true`. If a case ends up with fewer than 3 valid
  measured runs, the case is dropped from the mode's mean. If a mode ends up
  with no valid cases, the mode is marked `incomplete`.

## Output

One JSON file per engine: `results/<engine>.json`. Multiple versions
accumulate in `versions`. Re-running a version overwrites that version's
block; other versions are untouched.

```json
{
  "name":    "stockfish",
  "owner":   "official-stockfish",
  "repo":    "https://github.com/official-stockfish/Stockfish",
  "versions": {
    "17": {
      "ran_at": "2026-05-23T17:14:00Z",
      "disqualified": false,
      "modes": {
        "single-no-cache": {
          "mean_nps": 123456789,
          "positions": [
            {
              "name": "startpos",
              "fen": "rnbqkbnr/...",
              "depth": 4,
              "expected_nodes": 197281,
              "runs": [
                { "nodes": 197281, "elapsed_sec": 0.012, "nps": 16440000 }
              ],
              "median_nps": 16500000
            }
          ]
        }
      }
    },
    "16.1": { "...": "historic, untouched" }
  }
}
```

Disqualified version block:

```json
"17": {
  "ran_at": "...",
  "disqualified": true,
  "reason": "wrong node count",
  "failed_case": {
    "mode": "multi-with-cache",
    "fen": "r3k2r/p1ppqpb1/...",
    "depth": 6,
    "expected_nodes": 8031647685,
    "captured_stdout": "...the engine's last few lines for debugging..."
  }
}
```

## Aggregator

`perft_war.py aggregate` walks `results/*.json`, picks the **most recent
non-disqualified version per engine**, writes `results/leaderboard.json`
with a flat list of `{engine, version, mode, mean_nps}` rows ready for the
website.

## CLI

```
perft_war.py run <engine.json> [--reps 5] [--warmups 1] [--depths 4,6]
                                [--modes single-no-cache,multi-with-cache]
                                [--timeout 600] [--results-dir results]

perft_war.py aggregate          [--results-dir results]
                                [--out results/leaderboard.json]
```

Defaults shown match the agreed plan. CLI flags exist mostly for debugging
(e.g. `--modes single-no-cache --reps 1` for a fast smoke test).

## Open items to resolve next session

1. **Pos4 FEN ambiguity.** `Pp1P2PP` vs canonical `Pp1P2pP`. Pick one,
   re-verify D4 + D6 against a second engine (Stockfish), commit.
2. **Pos5 D6 cross-check.** 3,048,196,529 came from MoveGen.App alone — get
   a second engine (Stockfish) to confirm before committing as reference.
3. **Pos3 D4 cross-check.** 43,238 comes from MoveGen.App; matches the
   widely-published value, but not in the bundled EPDs we have. A second
   engine confirmation would close it out.
4. **Example engine descriptor.** Ship `engines/example-stockfish.json` with
   working commands so users have something to copy from.
5. **Per-engine wrapper-script convention.** Should the descriptor `command`
   strings point to wrapper scripts the engine maintainer ships (cleaner,
   makes the descriptor portable), or be raw shell with engine flags inline
   (less indirection)? Doesn't block implementation — both work; just a
   convention to settle.

## Directory layout (target)

```
Perft-War/
├── README.md                    # this file
├── perft_war.py                 # not written yet
├── engines/                     # one descriptor per engine
│   └── example-stockfish.json   # placeholder
└── results/                     # per-engine result files + leaderboard.json
    └── .gitkeep
```
