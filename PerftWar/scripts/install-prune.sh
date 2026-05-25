#!/usr/bin/env bash
# Clone, build, and verify Prune → bin/prune/core/prune.
#
# Prune (tgirolami09) is a UCI engine in C++. We track the `dev` branch
# because its latest commit speeds perft up versus main. Build steps per
# the README: cd core && make prune. The build picks up Fathom from a
# sibling directory in the same repo (no external dep).

ENGINE="prune"
REPO="https://github.com/tgirolami09/Prune"
BRANCH="dev"
ENGINE_DIR="bin/prune"
BINARY="$ENGINE_DIR/core/prune"

# shellcheck source=_common.sh
source "$(cd "$(dirname "$0")" && pwd)/_common.sh"

# --- Preflight -------------------------------------------------------------
command -v g++ >/dev/null 2>&1 || die "g++ not found (Prune's Makefile defaults to g++)"
command -v make >/dev/null 2>&1 || die "make not found"

# --- Clone (dev branch) ----------------------------------------------------
# `clone_or_keep` always clones the default branch; we need `dev`, so do
# the clone ourselves (still idempotent: keep an existing checkout).
if [ -d "$ENGINE_DIR/.git" ]; then
  log "$ENGINE_DIR already cloned (rm -rf to force a fresh checkout)"
  # If currently on a different branch, switch to dev.
  current=$(git -C "$ENGINE_DIR" rev-parse --abbrev-ref HEAD 2>/dev/null || echo "")
  if [ "$current" != "$BRANCH" ]; then
    log "switching to $BRANCH"
    git -C "$ENGINE_DIR" fetch --depth 1 origin "$BRANCH"
    git -C "$ENGINE_DIR" checkout "$BRANCH"
  else
    git -C "$ENGINE_DIR" pull --ff-only || true
  fi
else
  [ -d "$ENGINE_DIR" ] && rm -rf "$ENGINE_DIR"
  log "cloning $REPO ($BRANCH branch) into $ENGINE_DIR"
  git clone --depth 1 --branch "$BRANCH" "$REPO" "$ENGINE_DIR"
fi

# --- Build -----------------------------------------------------------------
HOST=$(detect_host)
log "building (host=$HOST, cd core && make prune)"
(
  cd "$ENGINE_DIR/core"
  make prune
)

[ -x "$BINARY" ] || die "expected binary at $BINARY but it's missing"

out=$(verify_perft "$BINARY") || die "perft test failed"

version=$(detect_version "$out")
if [ -n "$version" ]; then
  log "UCI banner: $version"
  log "→ consider setting engines/$ENGINE.json's \"version\" to: $version"
fi
commit=$(git -C "$ENGINE_DIR" rev-parse --short HEAD 2>/dev/null || echo "")
[ -n "$commit" ] && log "prune commit: $commit"

log "done. launch: $BINARY"
