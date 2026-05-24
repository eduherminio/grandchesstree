#!/usr/bin/env bash
# Clone, build, and verify perft_cpu_2026 → bin/perft_cpu_2026/build/perft_cpu.
#
# perft_cpu_2026 (Ankan Banerjee) is a perft-only CLI tool, not a UCI engine.
# PerftWar drives it via wrappers/perft_cpu_2026/run.sh. Build needs:
#   - CMake >= 3.18
#   - C++20 compiler (gcc >= 11, clang >= 14, or MSVC 2022)

ENGINE="perft_cpu_2026"
REPO="https://github.com/ankan-ban/perft_cpu_2026"
ENGINE_DIR="bin/perft_cpu_2026"
BINARY="$ENGINE_DIR/build/perft_cpu"
WRAPPER="wrappers/perft_cpu_2026/run.sh"

# shellcheck source=_common.sh
source "$(cd "$(dirname "$0")" && pwd)/_common.sh"

# --- Toolchain preflight ---------------------------------------------------
command -v cmake >/dev/null 2>&1 || die "cmake not found (need >= 3.18)"

# Need a C++20 compiler. Prefer the system default; verify by version.
if command -v g++ >/dev/null 2>&1; then
  gcc_major=$(g++ -dumpversion 2>/dev/null | cut -d. -f1)
  if [ -n "$gcc_major" ] && [ "$gcc_major" -ge 11 ]; then
    log "using g++ $gcc_major"
  else
    log "g++ found but version is too old ($gcc_major); needs >= 11 — will let CMake try anyway"
  fi
fi

# --- Clone + build ---------------------------------------------------------
clone_or_keep "$ENGINE_DIR" "$REPO"

[ -f "$WRAPPER" ] || die "wrapper missing at $WRAPPER"
chmod +x "$WRAPPER" 2>/dev/null || true

HOST=$(detect_host)
log "configuring (host=$HOST, cmake Release)"
(
  cd "$ENGINE_DIR"
  rm -rf build
  cmake -B build -DCMAKE_BUILD_TYPE=Release
  cmake --build build --config Release -j
)

# Probe for the produced binary — CMake's output name depends on the project's
# CMakeLists. Default expected: build/perft_cpu (or perft_cpu_2026, etc.).
if [ ! -x "$BINARY" ]; then
  alt=$(find "$ENGINE_DIR/build" -maxdepth 2 -type f -perm -111 -name 'perft_cpu*' 2>/dev/null | head -1 || true)
  if [ -n "$alt" ] && [ "$alt" != "$BINARY" ]; then
    log "found binary at $alt — symlinking to $BINARY"
    ln -sf "$(basename "$alt")" "$BINARY" || ln -sf "$alt" "$BINARY"
  fi
fi
[ -x "$BINARY" ] || die "expected binary at $BINARY but it's missing"

# Verify via the WRAPPER.
out=$(verify_perft "$WRAPPER") || die "perft test failed (via wrapper)"

commit=$(git -C "$ENGINE_DIR" rev-parse --short HEAD 2>/dev/null || echo "")
[ -n "$commit" ] && log "perft_cpu_2026 commit: $commit → consider updating engines/$ENGINE.json's \"version\""

log "done. launch: $WRAPPER (binary: $BINARY)"
