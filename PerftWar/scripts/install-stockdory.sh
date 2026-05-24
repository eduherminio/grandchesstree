#!/usr/bin/env bash
# Clone, build, and verify StockDory → bin/stockdory/Build/StockDory.
# StockDory's README mandates a fairly recent toolchain:
#   - CMake >= 3.21
#   - Clang (LLVM) >= 20.0.0
#   - Ninja >= 1.10.2
# This script does a hard preflight on those — failing fast with a clear
# message is far less painful than a 200-line cmake spew.

ENGINE="stockdory"
REPO="https://github.com/TheBlackPlague/StockDory"
ENGINE_DIR="bin/stockdory"
BINARY="$ENGINE_DIR/Build/StockDory"

# shellcheck source=_common.sh
source "$(cd "$(dirname "$0")" && pwd)/_common.sh"

# --- Toolchain preflight ----------------------------------------------------
command -v cmake >/dev/null 2>&1 || die "cmake not found (need >= 3.21)"
command -v ninja >/dev/null 2>&1 || die "ninja not found (install via apt/brew)"
if ! command -v clang >/dev/null 2>&1; then
  die "clang not found (need >= 20.0.0; StockDory uses C++20 features)"
fi
clang_major=$(clang --version | head -1 | sed -nE 's/.*version ([0-9]+).*/\1/p')
if [ -z "$clang_major" ] || [ "$clang_major" -lt 20 ]; then
  die "clang version ${clang_major:-?} is too old; StockDory requires >= 20"
fi

# --- Clone + build ----------------------------------------------------------
clone_or_keep "$ENGINE_DIR" "$REPO"

HOST=$(detect_host)
log "configuring (host=$HOST, cmake + ninja, clang)"
(
  cd "$ENGINE_DIR"
  rm -rf Build
  cmake -B Build -DCMAKE_BUILD_TYPE=Release \
        -DCMAKE_C_COMPILER=clang \
        -DCMAKE_CXX_COMPILER=clang++ \
        -G Ninja
  cmake --build Build --config Release
)

[ -x "$BINARY" ] || die "expected binary at $BINARY but it's missing"

out=$(verify_perft "$BINARY") || die "perft test failed"

version=$(detect_version "$out")
if [ -n "$version" ]; then
  log "UCI banner: $version"
  log "→ consider setting engines/$ENGINE.json's \"version\" to: $version"
fi

# StockDory doesn't tag releases the way most engines do — pin to git commit
# for reproducibility.
commit=$(git -C "$ENGINE_DIR" rev-parse --short HEAD 2>/dev/null || echo "")
[ -n "$commit" ] && log "stockdory commit: $commit"

log "done. launch: $BINARY"
