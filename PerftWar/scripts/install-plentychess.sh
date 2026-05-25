#!/usr/bin/env bash
# Clone, build, and verify PlentyChess → bin/plentychess/plentychess.
#
# PlentyChess (Yoshie2000) is a full UCI engine in C++. Build:
#   make profile-build EXE=plentychess arch=bmi2
# `EXE` controls the output binary name. We default to arch=bmi2 (Haswell-class
# + BMI2/PEXT, works on Haswell+ Intel and Zen2+ AMD); override with
# `PLENTYCHESS_ARCH=avx512vnni` on Cascade Lake / Zen4 for a bit more.
#
# NNUE: the Makefile auto-downloads the .nnue from PlentyNetworks releases
# based on network.txt — no manual step needed.

ENGINE="plentychess"
REPO="https://github.com/Yoshie2000/PlentyChess"
ENGINE_DIR="bin/plentychess"
BINARY="$ENGINE_DIR/plentychess"
ARCH="${PLENTYCHESS_ARCH:-bmi2}"

# shellcheck source=_common.sh
source "$(cd "$(dirname "$0")" && pwd)/_common.sh"

# --- Preflight -------------------------------------------------------------
command -v make >/dev/null 2>&1 || die "make not found"
command -v clang++ >/dev/null 2>&1 || command -v g++ >/dev/null 2>&1 \
  || die "no C++ compiler (clang++/g++) found"
command -v curl >/dev/null 2>&1 || die "curl not found (Makefile downloads NNUE)"

# --- Clone -----------------------------------------------------------------
clone_or_keep "$ENGINE_DIR" "$REPO"

# --- Build (with PGO) ------------------------------------------------------
HOST=$(detect_host)
log "building (host=$HOST, arch=$ARCH, profile-build, EXE=plentychess)"
(
  cd "$ENGINE_DIR"
  make -j profile-build EXE=plentychess arch="$ARCH"
)

[ -x "$BINARY" ] || die "expected binary at $BINARY but it's missing"

out=$(verify_perft "$BINARY") || die "perft test failed"

version=$(detect_version "$out")
if [ -n "$version" ]; then
  log "UCI banner: $version"
  log "→ consider setting engines/$ENGINE.json's \"version\" to: $version"
fi
commit=$(git -C "$ENGINE_DIR" rev-parse --short HEAD 2>/dev/null || echo "")
[ -n "$commit" ] && log "plentychess commit: $commit"

log "done. launch: $BINARY"
