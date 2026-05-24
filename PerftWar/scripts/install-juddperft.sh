#!/usr/bin/env bash
# Clone, build, and verify Juddperft → bin/juddperft/juddperft-gcc.
#
# Juddperft (Judd Niemann) is an xboard-style interactive perft tool, not a
# UCI engine. The README's recommended Linux build is a one-liner:
#   g++ -pthread -std=c++11 *.cpp -o ./juddperft-gcc -latomic -O3
# We follow that — no Qt or external deps needed.

ENGINE="juddperft"
REPO="https://github.com/jniemann66/juddperft"
ENGINE_DIR="bin/juddperft"
BINARY="$ENGINE_DIR/juddperft-gcc"
WRAPPER="wrappers/juddperft/run.sh"

# shellcheck source=_common.sh
source "$(cd "$(dirname "$0")" && pwd)/_common.sh"

clone_or_keep "$ENGINE_DIR" "$REPO"

[ -f "$WRAPPER" ] || die "wrapper missing at $WRAPPER"
chmod +x "$WRAPPER" 2>/dev/null || true

# Locate the .cpp files — juddperft has historically had them either at the
# repo root or in src/. Probe.
SRC_DIR="$ENGINE_DIR"
if compgen -G "$ENGINE_DIR/src/*.cpp" >/dev/null 2>&1; then
  SRC_DIR="$ENGINE_DIR/src"
fi

log "building (g++ -pthread -std=c++11 *.cpp from $SRC_DIR)"
(
  cd "$SRC_DIR"
  rm -f juddperft-gcc
  g++ -pthread -std=c++11 *.cpp -o juddperft-gcc -latomic -O3
)

# If we built inside src/, move the binary up to the descriptor's expected path.
if [ "$SRC_DIR" != "$ENGINE_DIR" ] && [ -x "$SRC_DIR/juddperft-gcc" ]; then
  mv -f "$SRC_DIR/juddperft-gcc" "$BINARY"
fi
[ -x "$BINARY" ] || die "expected binary at $BINARY but it's missing"

# Verify via the WRAPPER (single-threaded, defaults).
out=$(verify_perft "$WRAPPER") || die "perft test failed (via wrapper)"

commit=$(git -C "$ENGINE_DIR" rev-parse --short HEAD 2>/dev/null || echo "")
[ -n "$commit" ] && log "juddperft commit: $commit → consider updating engines/$ENGINE.json's \"version\""

log "done. launch: $WRAPPER (binary: $BINARY)"
