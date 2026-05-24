#!/usr/bin/env bash
# UCI wrapper around jniemann66/juddperft (xboard-style interactive perft).
#
# Juddperft's native commands are `setboard <fen>`, `perftfast <depth>`,
# and `cores <n>`. This wrapper:
#   1. Reads UCI commands off stdin from PerftWar.
#   2. On each `go perft <n>`, spawns a fresh juddperft, pipes in:
#         cores <CORES>
#         setboard <FEN>
#         perftfast <DEPTH>
#         quit
#      and forwards its stdout. Then prints `perft-done` as the end marker.
#
# **TT-warmth caveat:** juddperft is designed to be persistent so its
# transposition table warms across calls. This wrapper re-spawns per call,
# so the "with-cache" mode here measures cold-TT performance, not warm-TT.
# For a real warm-TT comparison, either drive juddperft natively or write
# a more elaborate keep-alive wrapper.

set -u

HERE="$(cd "$(dirname "$0")" && pwd)"
JUDDPERFT_BIN="$HERE/../../bin/juddperft/juddperft-gcc"

# Mode args: --cores=N
CORES=1
for arg in "$@"; do
  case "$arg" in
    --cores=*) CORES="${arg#--cores=}" ;;
  esac
done

FEN="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

while IFS= read -r line; do
  case "$line" in
    uci)
      echo "id name juddperft-wrapper"
      echo "id author juddperft UCI wrapper"
      echo "uciok"
      ;;
    isready)
      echo "readyok"
      ;;
    ucinewgame|"position startpos"*)
      FEN="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
      ;;
    "position fen "*)
      rest="${line#position fen }"
      FEN="${rest%% moves*}"
      ;;
    "go perft "*|"perft "*)
      depth="${line##* }"
      {
        echo "cores $CORES"
        echo "setboard $FEN"
        echo "perftfast $depth"
        echo "quit"
      } | "$JUDDPERFT_BIN" 2>&1
      echo "perft-done"
      ;;
    quit)
      break
      ;;
  esac
done
