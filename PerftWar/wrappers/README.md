# Engine wrappers

Some entries on the PerftWar leaderboard aren't full chess engines — they're
**move-generator libraries** that don't ship a UCI-speaking executable on
their own. PerftWar's harness drives every entry through the same
subprocess + stdin/stdout protocol, so each library needs a thin shim
binary that:

1. Loops on stdin reading UCI-style commands.
2. Handles `position startpos | position fen <fen>` and `go perft <n>` / `perft <n>`.
3. Calls the library's perft and writes `Nodes searched: <count>` to stdout.

Those shims live here. Each is intentionally small (~60 lines) and does
**no work of its own** — it just plumbs FEN → library function → node count.

## Why a wrapper, not in-process FFI?

PerftWar measures every entry the same way: spawn a process, send a
position, time the perft response. Calling libraries in-process (PyO3,
ctypes, etc.) would shave ~50–200 ms of subprocess startup off the
library entries only — biasing the leaderboard against every full
engine that *does* pay that cost. Wrappers keep every measurement on
the same footing.

## Methodology

All wrappers bulk-count at depth 1 (`movelist.len()`), matching the
optimisation Stockfish and similar engines use in their own perft. The
wrapper therefore measures the library's pure move-generation
throughput, **not** the cost of recursing one extra level just to count
leaves.

## Wrappers

| Wrapper | Language | Library | Binary path | Source |
|---|---|---|---|---|
| `cozy-chess/` | Rust | cozy-chess | `target/release/cozy-perft` | crates.io: `cozy-chess` |
| `shakmaty/` | Rust | shakmaty | `target/release/shakmaty-perft` | crates.io: `shakmaty` |
| `jordanbray-chess/` | Rust | jordanbray's chess | `target/release/jordan-perft` | crates.io: `chess` |

The Rust wrappers pull their library from crates.io as a normal
Cargo dependency — no clone needed.

## UCI subset implemented

Each wrapper implements only what PerftWar drives:

| Command | Handling |
|---|---|
| `uci` | Replies with `id name`, `id author`, `uciok`. |
| `isready` | Replies with `readyok`. |
| `ucinewgame` | Resets to startpos. |
| `position startpos` | Sets startpos. (Any trailing `moves …` is ignored — PerftWar never sends moves.) |
| `position fen <fen> [moves …]` | Sets the given FEN; trailing moves ignored. |
| `go perft <n>` / `perft <n>` | Runs perft, prints `Nodes searched: <count>` on its own line. |
| `quit` | Exits cleanly. |
| Anything else | Silently ignored. |

The engine descriptors in `engines/*.json` for these wrappers therefore
only define a `single-no-cache` mode — bare movegen libraries have no
TT and no built-in threading.

## Known caveats

- **Surge on Apple Silicon may not build.** Surge uses x86 PEXT/BMI
  intrinsics in its movegen and has no arm64 path. Test on Linux x86_64
  if the Mac build fails.
- **Library API drift.** The Rust wrappers pin minor versions in
  `Cargo.toml` (`cozy-chess = "0.3"`, `shakmaty = "0.27"`, `chess = "3"`).
  If a library breaks API in a future minor version, the wrapper may
  need a small edit.
- **Wrappers don't expose library version in their UCI banner.** The
  install scripts read the resolved version from `Cargo.lock` (or the
  surge git commit) instead and print it so you can update the
  matching `engines/<lib>.json`'s `"version"` field.

## Adding a new wrapper

Pattern from any of the existing wrappers:

1. `wrappers/<lib>/Cargo.toml` (or Makefile) declaring the library
   dependency and the binary name.
2. `wrappers/<lib>/src/main.rs` (or `wrapper.cpp`) implementing the
   UCI subset above plus a perft function that bulk-counts at depth 1.
3. `scripts/install-<lib>.sh` that builds the wrapper and runs
   `verify_perft` from `scripts/_common.sh`.
4. `engines/<lib>.json` pointing `launch` at the resulting binary,
   declaring only `single-no-cache` mode.
