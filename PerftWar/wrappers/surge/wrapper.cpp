// Minimal UCI wrapper around surge's movegen library.
//
// Surge is a flat C++ source tree, not a packaged library — the Makefile
// here compiles wrapper.cpp alongside surge's own .cpp files into a single
// `surge-perft` binary. Surge's own `perft.cpp` ships with its own main()
// running a hardcoded set of test positions; the Makefile excludes it from
// the link so this file's main() wins.
//
// See PerftWar/wrappers/README.md for the design rationale and the exact
// UCI subset implemented.

#include <cstdlib>
#include <iostream>
#include <string>

#include "position.h"

namespace {

const std::string DEFAULT_FEN =
    "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

// Recursive perft, templated on side-to-move (surge's MoveList / play /
// undo are all templated on Color). Bulk-counts at depth 1 via
// MoveList::size() — matches Stockfish's own perft optimisation.
template <Color Us>
unsigned long long perft(Position& p, unsigned depth) {
    if (depth == 0) return 1;
    MoveList<Us> list(p);
    if (depth == 1) return list.size();
    unsigned long long n = 0;
    for (Move m : list) {
        p.play<Us>(m);
        n += perft<~Us>(p, depth - 1);
        p.undo<Us>(m);
    }
    return n;
}

// Dispatcher: pick the right template instantiation based on whose turn
// it is in the parsed position.
unsigned long long do_perft(Position& p, unsigned d) {
    return p.turn() == WHITE ? perft<WHITE>(p, d) : perft<BLACK>(p, d);
}

bool starts_with(const std::string& s, const char* prefix) {
    return s.rfind(prefix, 0) == 0;
}

} // namespace

int main() {
    Position p;
    Position::set(DEFAULT_FEN, p);

    std::string line;
    while (std::getline(std::cin, line)) {
        if (line == "uci") {
            std::cout << "id name surge-perft\n"
                      << "id author surge UCI wrapper\n"
                      << "uciok\n";
        } else if (line == "isready") {
            std::cout << "readyok\n";
        } else if (line == "ucinewgame" || starts_with(line, "position startpos")) {
            Position::set(DEFAULT_FEN, p);
        } else if (starts_with(line, "position fen ")) {
            std::string fen = line.substr(std::string("position fen ").size());
            // Drop any " moves …" tail — PerftWar never sends moves.
            auto m = fen.find(" moves");
            if (m != std::string::npos) fen = fen.substr(0, m);
            Position::set(fen, p);
        } else if (starts_with(line, "go perft ") || starts_with(line, "perft ")) {
            std::string num = starts_with(line, "go perft ")
                ? line.substr(std::string("go perft ").size())
                : line.substr(std::string("perft ").size());
            unsigned depth = static_cast<unsigned>(std::atoi(num.c_str()));
            std::cout << "Nodes searched: " << do_perft(p, depth) << "\n";
        } else if (line == "quit") {
            break;
        }
        // Anything else (setoption, etc.) silently ignored.
        std::cout.flush();
    }
    return 0;
}
