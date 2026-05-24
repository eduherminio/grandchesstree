//! Minimal UCI wrapper around the shakmaty crate.
//!
//! See PerftWar/wrappers/README.md for the design rationale and the exact
//! UCI subset implemented. This file does nothing of substance other than:
//!   - Parse a tiny set of UCI commands off stdin.
//!   - Call `shakmaty::Chess` to set up the position and generate moves.
//!   - Recurse, bulk-counting at depth 1 to match Stockfish's own perft.

use std::io::{self, BufRead, Write};

use shakmaty::fen::Fen;
use shakmaty::{CastlingMode, Chess, Position};

/// Recursive perft. Uses `legal_moves().len()` at depth 1 for bulk
/// counting (matches Stockfish's own perft optimisation).
fn perft(pos: &Chess, depth: u32) -> u64 {
    if depth == 0 {
        return 1;
    }
    let moves = pos.legal_moves();
    if depth == 1 {
        return moves.len() as u64;
    }
    let mut nodes = 0u64;
    for mv in moves {
        let mut child = pos.clone();
        child.play_unchecked(&mv);
        nodes += perft(&child, depth - 1);
    }
    nodes
}

fn parse_fen(s: &str) -> Option<Chess> {
    let fen: Fen = s.parse().ok()?;
    fen.into_position(CastlingMode::Standard).ok()
}

fn main() {
    let stdin = io::stdin();
    let mut out = io::stdout().lock();
    let mut pos = Chess::default();

    for line in stdin.lock().lines() {
        let Ok(line) = line else { break };
        let cmd = line.trim();

        if cmd == "uci" {
            let _ = writeln!(out, "id name shakmaty-perft");
            let _ = writeln!(out, "id author shakmaty UCI wrapper");
            let _ = writeln!(out, "uciok");
        } else if cmd == "isready" {
            let _ = writeln!(out, "readyok");
        } else if cmd == "ucinewgame" || cmd.starts_with("position startpos") {
            pos = Chess::default();
        } else if let Some(rest) = cmd.strip_prefix("position fen ") {
            // Drop any trailing " moves …" — PerftWar never plays moves.
            let fen = rest.split(" moves").next().unwrap_or(rest).trim();
            if let Some(p) = parse_fen(fen) {
                pos = p;
            }
        } else if cmd.starts_with("go perft ") || cmd.starts_with("perft ") {
            let depth_str = cmd
                .strip_prefix("go perft ")
                .or_else(|| cmd.strip_prefix("perft "))
                .unwrap_or("0")
                .trim();
            if let Ok(d) = depth_str.parse::<u32>() {
                let _ = writeln!(out, "Nodes searched: {}", perft(&pos, d));
            }
        } else if cmd == "quit" {
            break;
        }
        let _ = out.flush();
    }
}
