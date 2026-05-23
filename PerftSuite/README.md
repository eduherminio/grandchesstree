# perftcheck

A cross-platform CLI that validates a UCI chess engine's `go perft N` output against ~7,000 known-correct positions and writes a JSON report.

Documented for users at [grandchesstree.com/perftcheck.html](https://grandchesstree.com/perftcheck.html). The release workflow at `.github/workflows/publish.yml` builds and uploads a self-contained binary per platform to GitHub Releases on every tagged release — `perftcheck-linux-x64`, `perftcheck-osx-arm64`, `perftcheck-win-x64.exe`, etc.

## Quick start

Pre-built binaries on [GitHub Releases](https://github.com/Timmoth/grandchesstree/releases/latest); see the install table on the website for direct download links. From source:

```sh
perftcheck --engine /path/to/engine
```

That runs every bundled position up to depth 4 (~28k cases), prints a live progress + summary table, and writes `perft-report.json` to the current directory. Exit code is `0` if every case passed, `1` otherwise (which is what you want from CI).

## Options

| Flag                       | Default                | Meaning                                                                 |
|----------------------------|------------------------|-------------------------------------------------------------------------|
| `-e, --engine <PATH>`      | *required*             | UCI engine executable                                                   |
| `--epd <FILE>`             | bundled (3 corpora)    | Replace the bundled corpora with this file. Repeatable.                 |
| `--depth-cap <N>`          | `4`                    | Skip cases deeper than this                                             |
| `--depth-min <N>`          | `1`                    | Skip cases shallower than this                                          |
| `--timeout <SECS>`         | `30`                   | Per-case timeout. Engine is killed and restarted if exceeded.           |
| `--filter <SUBSTR>`        | —                      | Only run cases whose FEN contains this substring                        |
| `--limit <N>`              | —                      | Take only the first N matching cases                                    |
| `--report <PATH>`          | `perft-report.json`    | Path to write the JSON report                                           |
| `--fail-fast`              | off                    | Stop on the first non-pass                                              |
| `--quiet`                  | off                    | Suppress live console output (CI mode)                                  |

## Bundled corpora

Three EPD files are baked into the binary as embedded resources:

| Name                                  | Source                                  | Positions |
|---------------------------------------|-----------------------------------------|----------:|
| `perft.epd`                           | Chris Whittington collection            | 173       |
| `perft-marcel.epd`                    | Marcel van Kervinck via CW              | 6,837     |
| `ferdy_perft_single_check_1…19.epd`   | Ferdinand Mosca (single-check edge cases) | 93,811    |
| `ferdy_perft_enpassant_1.epd`         | Ferdinand Mosca (en-passant edge cases)   | 3,760     |
| `ferdy_perft_double_checks.epd`       | Ferdinand Mosca (double-check edge cases) | 184       |

All sourced from [ChrisWhittington/Chess-EPDs](https://github.com/ChrisWhittington/Chess-EPDs), released into the public domain.

Format: `<FEN> ; D1 <count> ; … ; Dn <count>` (CW/Marcel files carry D1–D6; the Ferdy files carry D4 only). Together they make ~105k position/depth cases — most at depth 4.

## Engine protocol

Standard UCI:

```
→ uci
←   id name <engine>
←   …
←   uciok
→ isready
←   readyok
→ position fen <fen>
→ go perft <depth>
←   …
←   Nodes searched: <total>
→ isready
←   readyok
…
→ quit
```

The tool keeps one engine subprocess warm and feeds positions through it. If a `go perft N` doesn't return a `Nodes searched:` line within `--timeout` seconds the engine is killed, the case is recorded as a timeout, and a fresh subprocess is spun up for the next case.

`Total nodes: N` and `Total: N` are also accepted as fallback formats for engines that don't follow Stockfish's exact wording.

## JSON report shape

```json
{
  "tool":      "perftcheck",
  "version":   "0.1.0",
  "engine":    "/abs/path/to/engine",
  "engineId":  "Stockfish 17",
  "startedUtc": "2026-05-23T15:42:11Z",
  "durationSeconds": 21.94,
  "options": {
    "depthMin": 1, "depthCap": 4, "timeoutSeconds": 30,
    "epdFiles": ["perft.epd", "perft-marcel.epd", "ferdy_perft_single_check_1.epd", "…"],
    "failFast": false
  },
  "totals":   { "cases": 104765, "passed": 104763, "failed": 2, "timeout": 0, "error": 0 },
  "failures": [
    {
      "kind":     "mismatch",
      "fen":      "rnbqkbnr/…",
      "depth":    4,
      "expected": 197281,
      "actual":   197280,
      "diff":     -1,
      "elapsedSeconds": 0.014,
      "source":   "perft.epd:1"
    }
  ]
}
```

Failure kinds:
- `"mismatch"` — node count differs from expected (`expected`, `actual`, `diff` fields populated).
- `"timeout"` — engine didn't respond within `--timeout` seconds.
- `"error"`   — engine pipe closed or output unparseable (raw output captured in `engineOutput`, truncated to ~2 KB).

`source` is `<epd-file>:<line>` so failures map directly back to a line you can grep.

## Common recipes

```sh
# quick smoke test: only the canonical 173 positions, depth ≤ 4
perftcheck --engine ./engine --epd perft.epd

# CI gate: full bundled corpus, machine-readable output
perftcheck --engine ./engine --quiet --report perft.json

# debug one suspect position
perftcheck --engine ./engine --filter "r3k2r/p1ppqpb1" --depth-cap 5

# overnight regression at depth 5 (slow)
perftcheck --engine ./engine --depth-cap 5 --timeout 120

# halt at the first divergence — for bisection runs
perftcheck --engine ./engine --fail-fast
```

## Building from source

```sh
dotnet build                                        # debug builds
dotnet run -- --engine /path/to/engine              # dev iteration

# Self-contained binaries for all platforms (~38 MB each, compressed)
./build/publish-all.sh                              # macOS / Linux
build\publish-all.cmd                               # Windows
```

Targets: `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`, `win-x64`. Each produces a single self-contained executable in `dist/<rid>/perftcheck[.exe]` — no .NET runtime required on the target machine.

Trimming is currently off because Spectre.Console.Cli's reflection-based command binding doesn't survive aggressive trim. With trimming you'd get ~14 MB binaries that throw at startup — not worth the size win.

## CI release

`.github/workflows/publish.yml` (job: `Build and Package PerftSuite`) builds the same five targets on every workflow-dispatch with the same `dotnet publish` flags used here. Output filenames are deliberately version-less (`perftcheck-osx-arm64`, etc.) so the `/releases/latest/download/perftcheck-<rid>` URLs on the website stay valid release-over-release.

## What's not in scope (yet)

- Parallel runner — currently one engine subprocess at a time. Useful for engines that take seconds per case; the bundled corpus at depth 4 runs in ~22 s on a fast engine so parallelism isn't urgent.
- A `divide` sub-command to diff engine vs reference move-by-move. The JSON failures already carry enough info to manually run divide; not worth the moving parts yet.
- A self-test mode that validates the bundled EPDs themselves. The corpora are public/long-published; trust them.
- Engine protocols other than UCI. Every modern engine speaks UCI; engines that don't are unusual enough to wrap with a 10-line UCI adapter.

## Engine compatibility notes

- **Stockfish** — works out of the box. Uses `Nodes searched: N`.
- **Komodo, Ethereal, Berserk, Lc0** — UCI standard, work out of the box.
- **GrandChessTree.Engine** (this repo) — uses a *non-UCI* colon-delimited command protocol. To validate it you'd either teach it `uci` / `go perft` (a few dozen lines) or write a thin shell-script adapter.
- **MoveGen** (this repo, `MoveGen/`) — adds UCI mode to the demo Program.cs. Pointed at `movegen-engine.sh` it passes all 28,375 bundled cases at depth 4 in ~22 s.
