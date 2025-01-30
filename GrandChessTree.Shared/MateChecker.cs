using System.Runtime.CompilerServices;
using GrandChessTree.Shared.Tables;

namespace GrandChessTree.Shared;

public static unsafe class MateChecker
{
    public static bool WhiteCanEvadeCheck(ref Board board)
    {
        if (CanWhiteKingMove(ref board))
            return true;

        var pinMask = board.WhiteKingPinnedRay();

        var positions = board.WhiteKnight;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            if ((pinMask & (1ul << index)) != 0)
            {
                continue;
            }

            var potentialMoves = *(AttackTables.KnightAttackTable + index) & board.MoveMask;
            // Return true if there are any valid capture or push moves available.
            if ((potentialMoves & ~board.White) != 0)
            {
                return true;
            }
        }


        positions = board.WhiteBishop;
        while (positions != 0)
        {
            var index = positions.PopLSB();

            var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & board.MoveMask;

            if ((pinMask & (1ul << index)) != 0)
            {
                potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
            }
            // Return true if there are any valid capture or push moves available.
            if ((potentialMoves & ~board.White) != 0)
            {
                return true;
            }
        }

        positions = board.WhiteRook;
        while (positions != 0)
        {
            var index = positions.PopLSB();

            var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & board.MoveMask;

            if ((pinMask & (1ul << index)) != 0)
            {
                potentialMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
            }

            if ((potentialMoves & ~board.White) != 0)
            {
                return true;
            }
        }

        positions = board.WhiteQueen;
        while (positions != 0)
        {
            var index = positions.PopLSB();

            var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                     AttackTables.PextRookAttacks(board.Occupancy, index)) & board.MoveMask;

            if ((pinMask & (1ul << index)) != 0)
            {
                potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index) | AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
            }
            // Return true if there are any valid capture or push moves available.
            if ((potentialMoves & ~board.White) != 0)
            {
                return true;
            }
        }

        positions = board.WhitePawn;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            if (CanWhitePawnMove(ref board, index, (pinMask & (1ul << index)) != 0))
                return true;
        }

        return false;
    }

    public static bool BlackCanEvadeSingleCheck(ref Board board)
    {
        if (CanBlackKingMove(ref board))
            return true;

        var pinMask = board.BlackKingPinnedRay();

        var positions = board.BlackKnight;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            if ((pinMask & (1ul << index)) != 0)
            {
                continue;
            }

            var potentialMoves = *(AttackTables.KnightAttackTable + index) & board.MoveMask;
            // Return true if there are any valid capture or push moves available.
            if ((potentialMoves & ~board.Black) != 0)
            {
                return true;
            }
        }


        positions = board.BlackBishop;
        while (positions != 0)
        {
            var index = positions.PopLSB();

            var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & board.MoveMask;

            if ((pinMask & (1ul << index)) != 0)
            {
                potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
            }
            // Return true if there are any valid capture or push moves available.
            if ((potentialMoves & ~board.Black) != 0)
            {
                return true;
            }
        }

        positions = board.BlackRook;
        while (positions != 0)
        {
            var index = positions.PopLSB();

            var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & board.MoveMask;

            if ((pinMask & (1ul << index)) != 0)
            {
                potentialMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
            }

            if ((potentialMoves & ~board.Black) != 0)
            {
                return true;
            }
        }

        positions = board.BlackQueen;
        while (positions != 0)
        {
            var index = positions.PopLSB();

            var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                     AttackTables.PextRookAttacks(board.Occupancy, index)) & board.MoveMask;

            if ((pinMask & (1ul << index)) != 0)
            {
                potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index) | AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
            }
            // Return true if there are any valid capture or push moves available.
            if ((potentialMoves & ~board.Black) != 0)
            {
                return true;
            }
        }

        positions = board.BlackPawn;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            if (CanBlackPawnMove(ref board, index, (pinMask & (1ul << index)) != 0))
                return true;
        }


        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanWhitePawnMove(ref Board board, int index, bool isPinned)
    {
        var validMoves = AttackTables.WhitePawnAttackTable[index] & board.MoveMask & board.Black;
        if (isPinned)
        {
            validMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
        }

        if (validMoves != 0)
        {
            return true;
        }

        validMoves = AttackTables.WhitePawnPushTable[index] & board.MoveMask & ~board.Occupancy;
        if (isPinned)
        {
            validMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }

        var rankIndex = index.GetRankIndex();
        while (validMoves != 0)
        {
            var toSquare = validMoves.PopLSB();

            if (rankIndex.IsSecondRank() && toSquare.GetRankIndex() == 3)
            {
                // Double push: Check intermediate square
                var intermediateSquare = (index + toSquare) / 2; // Midpoint between start and destination
                if ((board.Occupancy & (1UL << intermediateSquare)) != 0)
                {
                    continue; // Intermediate square is blocked, skip this move
                }
                return true;
            }
            else
            {
                // single push
                return true;
            }
        }

        if (board.EnPassantFile != 8 && rankIndex.IsWhiteEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            Board newBoard = default;
            board.CloneTo(ref newBoard);
            var toSquare = Board.WhiteEnpassantOffset + board.EnPassantFile;

            newBoard.WhitePawn_Enpassant(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanWhiteKingMove(ref Board board)
    {
        var potentialMoves = *(AttackTables.KingAttackTable + board.WhiteKingPos) & ~board.WhiteKingDangerSquares();
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.White) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanBlackPawnMove(ref Board board, int index, bool isPinned)
    {
        var validMoves = AttackTables.BlackPawnAttackTable[index] & board.MoveMask & board.White;
        if (isPinned)
        {
            validMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
        }

        if (validMoves != 0)
        {
            return true;
        }

        validMoves = AttackTables.BlackPawnPushTable[index] & board.MoveMask & ~board.Occupancy;
        if (isPinned)
        {
            validMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }
        var rankIndex = index.GetRankIndex();

        while (validMoves != 0)
        {
            var toSquare = validMoves.PopLSB();

            if (rankIndex.IsSeventhRank() && toSquare.GetRankIndex() == 4)
            {
                // Double push: Check intermediate square
                var intermediateSquare = (index + toSquare) / 2; // Midpoint between start and destination
                if ((board.Occupancy & (1UL << intermediateSquare)) != 0)
                {
                    continue; // Intermediate square is blocked, skip this move
                }

                return true;
            }
            else
            {
                // single push
                return true;
            }
        }

        if (board.EnPassantFile != 8 && rankIndex.IsBlackEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            Board newBoard = default;
            board.CloneTo(ref newBoard);
            var toSquare = Board.blackEnpassantOffset + board.EnPassantFile;

            newBoard.BlackPawn_Enpassant(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanBlackKingMove(ref Board board)
    {
        var potentialMoves = *(AttackTables.KingAttackTable + board.BlackKingPos) & ~board.BlackKingDangerSquares();
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.Black) != 0;
    }
}