#!/usr/bin/env bash
# Clone, build, and verify Halogen → bin/halogen/build/Halogen.
# Halogen uses CMake (unlike most UCI engines on this list), so we run a
# fresh out-of-tree build under bin/halogen/build/.

ENGINE="halogen"
REPO="https://github.com/KierenP/Halogen"
ENGINE_DIR="bin/halogen"
BINARY="$ENGINE_DIR/build/Halogen"

# shellcheck source=_common.sh
source "$(cd "$(dirname "$0")" && pwd)/_common.sh"

clone_or_keep "$ENGINE_DIR" "$REPO"

HOST=$(detect_host)
log "building (host=$HOST, CMake Release)"
(
  cd "$ENGINE_DIR"
  rm -rf build
  mkdir build
  cd build
  cmake -DCMAKE_BUILD_TYPE=Release ..
  cmake --build . --parallel
)

# CMake may produce the binary under build/ or build/src/ depending on the
# project layout. Pick whichever it landed in.
if [ ! -x "$BINARY" ]; then
  alt=$(find "$ENGINE_DIR/build" -maxdepth 3 -type f -perm -111 -name 'Halogen*' 2>/dev/null | head -1 || true)
  if [ -n "$alt" ]; then
    log "found binary at $alt — symlinking to $BINARY"
    ln -sf "$(realpath "$alt" 2>/dev/null || cd "$(dirname "$alt")" && echo "$(pwd)/$(basename "$alt")")" "$BINARY"
  fi
fi
[ -x "$BINARY" ] || die "expected binary at $BINARY but it's missing"

out=$(verify_perft "$BINARY") || die "perft test failed"

version=$(detect_version "$out")
if [ -n "$version" ]; then
  log "UCI banner: $version"
  log "→ consider setting engines/$ENGINE.json's \"version\" to: $version"
fi

log "done. launch: $BINARY"
