using System.Runtime.CompilerServices;
using GrandChessTree.Shared.Tables;

namespace GrandChessTree.Shared;

public static unsafe class MateChecker
{
    public static bool WhiteCanEvadeCheck(ref Board board)
    {
        if (CanWhiteKingMove(ref board))
            return true;
        
        var positions = board.WhiteKnight;
        while (positions != 0)
            if (CanWhiteKnightMove(ref board, positions.PopLSB()))
                return true;

        positions = board.WhiteBishop;
        while (positions != 0)
            if (CanWhiteBishopMove(ref board, positions.PopLSB()))
                return true;

        positions = board.WhiteRook;
        while (positions != 0)
            if (CanWhiteRookMove(ref board, positions.PopLSB()))
                return true;

        positions = board.WhiteQueen;
        while (positions != 0)
            if (CanWhiteQueenMove(ref board, positions.PopLSB()))
                return true;

        positions = board.WhitePawn;
        while (positions != 0)
            if (CanWhitePawnMove(ref board, positions.PopLSB()))
                return true;
        
        return false;
    }

    public static bool WhiteCanEvadeDoubleCheck(ref Board board)
    {
        return CanWhiteKingMove(ref board);
    }

    public static bool BlackCanEvadeSingleCheck(ref Board board)
    {
        if (CanBlackKingMove(ref board))
            return true;
        
        var positions = board.BlackKnight;
        while (positions != 0)
            if (CanBlackKnightMove(ref board, positions.PopLSB()))
                return true;

        positions = board.BlackBishop;
        while (positions != 0)
            if (CanBlackBishopMove(ref board, positions.PopLSB()))
                return true;

        positions = board.BlackRook;
        while (positions != 0)
            if (CanBlackRookMove(ref board, positions.PopLSB()))
                return true;

        positions = board.BlackQueen;
        while (positions != 0)
            if (CanBlackQueenMove(ref board, positions.PopLSB()))
                return true;
        
        positions = board.BlackPawn;
        while (positions != 0)
            if (CanBlackPawnMove(ref board, positions.PopLSB()))
                return true;

        return false;
    }

    public static bool BlackCanEvadeDoubleCheck(ref Board board)
    {
        return CanBlackKingMove(ref board);
    }

    private static bool CanWhitePawnMove(ref Board board, int index)
    {
        Board newBoard = default;

        var rankIndex = index.GetRankIndex();
        var posEncoded = 1UL << index;
        int toSquare;

        if (board.EnPassantFile != 8 && rankIndex.IsWhiteEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            board.CloneTo(ref newBoard);
            toSquare = Board.BlackWhiteEnpassantOffset + board.EnPassantFile;

            newBoard.WhitePawn_Enpassant(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) return true;
        }

        var canPromote = rankIndex.IsSeventhRank();

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
    private static bool CanWhiteKnightMove(ref Board board, int index)
    {
        if ((board.PinMask & (1ul << index)) != 0)
        {
            return false;
        }

        var potentialMoves = *(AttackTables.KnightAttackTable + index) & (board.PushMask | board.CaptureMask);
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.White) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanWhiteBishopMove(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
        }
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.White) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanWhiteRookMove(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.White) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanWhiteQueenMove(ref Board board, int index)
    {
        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index) | AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.White) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanWhiteKingMove(ref Board board)
    {
        var potentialMoves = *(AttackTables.KingAttackTable + board.WhiteKingPos) & ~board.AttackedSquares;
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.White) != 0;
    }
    private static bool CanBlackPawnMove(ref Board board, int index)
    {
        Board newBoard = default;

        var rankIndex = index.GetRankIndex();
        var posEncoded = 1UL << index;
        int toSquare;

        if (board.EnPassantFile != 8 && rankIndex.IsBlackEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            board.CloneTo(ref newBoard);
            toSquare = Board.blackEnpassantOffset + board.EnPassantFile;

            newBoard.BlackPawn_Enpassant(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) return true;
        }

        var canPromote = rankIndex.IsSecondRank();

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
    private static bool CanBlackKnightMove(ref Board board, int index)
    {
        if ((board.PinMask & (1ul << index)) != 0)
        {
            return false;
        }

        var potentialMoves = *(AttackTables.KnightAttackTable + index) & (board.PushMask | board.CaptureMask);

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.Black) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanBlackBishopMove(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
        }

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.Black) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanBlackRookMove(ref Board board, int index)
    {
        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.Black) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanBlackQueenMove(ref Board board, int index)
    {
        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index) | AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }

        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.Black) != 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanBlackKingMove(ref Board board)
    {
        var potentialMoves = *(AttackTables.KingAttackTable + board.BlackKingPos) & ~board.AttackedSquares;
        // Return true if there are any valid capture or push moves available.
        return (potentialMoves & ~board.Black) != 0;
    }
}