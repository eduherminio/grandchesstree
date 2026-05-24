#!/usr/bin/env bash
# Clone surge into bin/surge, then build the wrapper at wrappers/surge → surge-perft.
# Surge isn't on a package registry, so we always clone the upstream repo.

ENGINE="surge"
REPO="https://github.com/nkarve/surge"
ENGINE_DIR="bin/surge"
WRAPPER_DIR="wrappers/surge"
BINARY="$WRAPPER_DIR/surge-perft"

# shellcheck source=_common.sh
source "$(cd "$(dirname "$0")" && pwd)/_common.sh"

clone_or_keep "$ENGINE_DIR" "$REPO"

[ -d "$WRAPPER_DIR" ] || die "wrapper source missing at $WRAPPER_DIR"
[ -f "$WRAPPER_DIR/Makefile" ] || die "wrapper not initialised — Makefile absent in $WRAPPER_DIR"

log "building wrapper"
(
  cd "$WRAPPER_DIR"
  make clean >/dev/null 2>&1 || true
  make
)

[ -x "$BINARY" ] || die "expected binary at $BINARY but it's missing"

out=$(verify_perft "$BINARY") || die "perft test failed"

# Surge has no version banner; pin the descriptor to the upstream commit.
commit=$(git -C "$ENGINE_DIR" rev-parse --short HEAD 2>/dev/null || echo "")
if [ -n "$commit" ]; then
  log "surge commit: $commit"
  log "→ consider setting engines/$ENGINE.json's \"version\" to: $commit"
fi

log "done. launch: $BINARY"
