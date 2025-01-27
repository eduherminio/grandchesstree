using System.Runtime.CompilerServices;
using GrandChessTree.Client.Tables;

namespace GrandChessTree.Client;

public static unsafe class MateChecker
{
    public const ulong BlackKingSideCastleRookPosition = 1UL << 63;
    public const ulong BlackKingSideCastleEmptyPositions = (1UL << 61) | (1UL << 62);
    public const ulong BlackQueenSideCastleRookPosition = 1UL << 56;
    public const ulong BlackQueenSideCastleEmptyPositions = (1UL << 57) | (1UL << 58) | (1UL << 59);

    public const ulong WhiteKingSideCastleRookPosition = 1UL << 7;
    public const ulong WhiteKingSideCastleEmptyPositions = (1UL << 6) | (1UL << 5);
    public const ulong WhiteQueenSideCastleRookPosition = 1UL;
    public const ulong WhiteQueenSideCastleEmptyPositions = (1UL << 1) | (1UL << 2) | (1UL << 3);

    public static void WhiteSingleCheckEvasionTest(ref Board board, ref Summary summary)
    {
        var positions = board.WhitePawn;
        while (positions != 0)
            if (PerftWhitePawn(ref board, positions.PopLSB()))
                return;

        positions = board.WhiteKnight;
        while (positions != 0)
            if (PerftWhiteKnight(ref board, positions.PopLSB()))
                return;

        positions = board.WhiteBishop;
        while (positions != 0)
            if (PerftWhiteBishop(ref board, positions.PopLSB()))
                return;

        positions = board.WhiteRook;
        while (positions != 0)
            if (PerftWhiteRook(ref board, positions.PopLSB()))
                return;

        positions = board.WhiteQueen;
        while (positions != 0)
            if (PerftWhiteQueen(ref board, positions.PopLSB()))
                return;

        positions = board.WhiteKing;
        while (positions != 0)
            if (PerftWhiteKing(ref board, positions.PopLSB()))
                return;

        summary.AddMate();
    }

    public static void WhiteDoubleCheckEvasionTest(ref Board board, ref Summary summary)
    {
        if (PerftWhiteKing(ref board, board.WhiteKingPos))
            return;

        summary.AddMate();
    }


    public static void BlackSingleCheckEvasionTest(ref Board board, ref Summary summary)
    {
        var positions = board.BlackPawn;
        while (positions != 0)
            if (PerftBlackPawn(ref board, positions.PopLSB()))
                return;

        positions = board.BlackKnight;
        while (positions != 0)
            if (PerftBlackKnight(ref board, positions.PopLSB()))
                return;

        positions = board.BlackBishop;
        while (positions != 0)
            if (PerftBlackBishop(ref board, positions.PopLSB()))
                return;

        positions = board.BlackRook;
        while (positions != 0)
            if (PerftBlackRook(ref board, positions.PopLSB()))
                return;

        positions = board.BlackQueen;
        while (positions != 0)
            if (PerftBlackQueen(ref board, positions.PopLSB()))
                return;

        positions = board.BlackKing;
        while (positions != 0)
            if (PerftBlackKing(ref board, positions.PopLSB()))
                return;

        summary.AddMate();
    }

    public static void BlackDoubleCheckEvasionTest(ref Board board, ref Summary summary)
    {
        if (PerftBlackKing(ref board, board.BlackKingPos))
            return;

        summary.AddMate();
    }

    public static bool PerftWhitePawn(ref Board board, int index)
    {
        Board newBoard = default;

        var rankIndex = index.GetRankIndex();
        var posEncoded = 1UL << index;

        if (board.EnPassantFile != 8 && rankIndex.IsWhiteEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_Enpassant(index);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
        }

        var canPromote = rankIndex.IsSeventhRank();
        int toSquare;

        // Take left piece
        var target = posEncoded.ShiftUpLeft();
        if ((board.Black & target) != 0)
        {
            toSquare = index.ShiftUpLeft();
            if (canPromote)
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_KnightPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
            }
        }

        // Take right piece
        target = posEncoded.ShiftUpRight();
        if ((board.Black & target) != 0)
        {
            toSquare = index.ShiftUpRight();
            if (canPromote)
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_KnightPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
            }
        }

        // Move up
        target = posEncoded.ShiftUp();
        if ((board.Occupancy & target) > 0)
            // Blocked from moving down
            return false;

        toSquare = index.ShiftUp();
        if (canPromote)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_KnightPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_BishopPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_RookPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_QueenPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;

            return false;
        }

        board.CloneTo(ref newBoard);
        newBoard.WhitePawn_Move(index, toSquare);
        if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
        target = target.ShiftUp();
        if (rankIndex.IsSecondRank() && (board.Occupancy & target) == 0)
        {
            // Double push
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_DoublePush(index, toSquare.ShiftUp());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftWhiteKnight(ref Board board, int index)
    {
        if ((board.PinMask & (1ul << index)) != 0)
        {
            return false;
        }

        var potentialMoves = *(AttackTables.KnightAttackTable + index) & (board.PushMask | board.CaptureMask);
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.Black | ~board.Occupancy)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftWhiteBishop(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
        }
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.Black | ~board.Occupancy)) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftWhiteRook(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.Black | ~board.Occupancy)) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftWhiteQueen(ref Board board, int index)
    {
        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index) | AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.Black | ~board.Occupancy)) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftWhiteKing(ref Board board, int index)
    {
        var potentialMoves = *(AttackTables.KingAttackTable + index) & ~board.AttackedSquares;
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.Black | ~board.Occupancy)) != 0;
    }


    public static bool PerftBlackPawn(ref Board board, int index)
    {
        Board newBoard = default;

        var rankIndex = index.GetRankIndex();
        var posEncoded = 1UL << index;

        if (board.EnPassantFile != 8 && rankIndex.IsBlackEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_Enpassant(index);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
        }

        var canPromote = rankIndex.IsSecondRank();
        int toSquare;

        // Left capture
        var target = posEncoded.ShiftDownLeft();
        if ((board.White & target) != 0)
        {
            toSquare = index.ShiftDownLeft();
            if (canPromote)
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_KnightPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            }
        }

        // Right capture
        target = posEncoded.ShiftDownRight();
        if ((board.White & target) != 0)
        {
            toSquare = index.ShiftDownRight();

            if (canPromote)
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_KnightPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            }
        }

        // Vertical moves
        target = posEncoded.ShiftDown();
        if ((board.Occupancy & target) > 0)
            // Blocked from moving down
            return false;

        toSquare = index.ShiftDown();
        if (canPromote)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_KnightPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_BishopPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_RookPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_QueenPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
            return false;
        }

        // Move down
        board.CloneTo(ref newBoard);
        newBoard.BlackPawn_Move(index, toSquare);
        if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
        target = target.ShiftDown();
        if (rankIndex.IsSeventhRank() && (board.Occupancy & target) == 0)
        {
            // Double push
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_DoublePush(index, toSquare.ShiftDown());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
        }

        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftBlackKnight(ref Board board, int index)
    {
        if ((board.PinMask & (1ul << index)) != 0)
        {
            return false;
        }

        var potentialMoves = *(AttackTables.KnightAttackTable + index) & (board.PushMask | board.CaptureMask);

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.White | ~board.Occupancy)) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftBlackBishop(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
        }

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.White | ~board.Occupancy)) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftBlackRook(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.White | ~board.Occupancy)) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftBlackQueen(ref Board board, int index)
    {
        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index) | AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.White | ~board.Occupancy)) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PerftBlackKing(ref Board board, int index)
    {
        var potentialMoves = *(AttackTables.KingAttackTable + index) & ~board.AttackedSquares;
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & (board.White | ~board.Occupancy)) != 0;
    }
}