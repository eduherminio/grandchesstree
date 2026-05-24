# Build the surge UCI wrapper into a single `surge-perft` binary.
#
# Run from this directory (scripts/install-surge.sh cd's here first):
#   make           # build
#   make clean     # remove the binary
#
# Surge is cloned into PerftWar/bin/surge/ by scripts/install-surge.sh; we
# pick up its source files from there.
#
# Note: surge's own src/perft.cpp ships with its own main() that runs a
# hardcoded test suite. We exclude it from the link so wrapper.cpp's main
# wins.

SURGE_DIR  := $(CURDIR)/../../bin/surge
SURGE_SRCS := $(filter-out $(SURGE_DIR)/src/perft.cpp, $(wildcard $(SURGE_DIR)/src/*.cpp))

CXX      ?= c++
CXXFLAGS ?= -O3 -std=c++17 -DNDEBUG -Wall -Wno-unused -I$(SURGE_DIR)/src

surge-perft: wrapper.cpp $(SURGE_SRCS)
	$(CXX) $(CXXFLAGS) wrapper.cpp $(SURGE_SRCS) -o $@

clean:
	rm -f surge-perft

.PHONY: clean
