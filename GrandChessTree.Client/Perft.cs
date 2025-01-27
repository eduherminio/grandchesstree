using System.Runtime.Intrinsics.X86;
using GrandChessTree.Client.Tables;

namespace GrandChessTree.Client;
public static unsafe class Perft
{
    public const ulong BlackKingSideCastleRookPosition = 1UL << 63;
    public const ulong BlackKingSideCastleEmptyPositions = (1UL << 61) | (1UL << 62);
    public const ulong BlackQueenSideCastleRookPosition = 1UL << 56;
    public const ulong BlackQueenSideCastleEmptyPositions = (1UL << 57) | (1UL << 58) | (1UL << 59);

    public const ulong WhiteKingSideCastleRookPosition = 1UL << 7;
    public const ulong WhiteKingSideCastleEmptyPositions = (1UL << 6) | (1UL << 5);
    public const ulong WhiteQueenSideCastleRookPosition = 1UL;
    public const ulong WhiteQueenSideCastleEmptyPositions = (1UL << 1) | (1UL << 2) | (1UL << 3);


    public static void PerftRoot(ref Board board, ref Summary summary, int depth, bool whiteToMove)
    {
        if (whiteToMove)
        {
            if (depth == 0)
            {
                summary.Nodes++;
                return;
            }

            board.Checkers = board.BlackCheckers();
            board.NumCheckers = ulong.PopCount(board.Checkers);
            board.AttackedSquares = board.WhiteKingDangerSquares();
            board.CaptureMask = 0xFFFFFFFFFFFFFFFF;
            board.PushMask = 0xFFFFFFFFFFFFFFFF;
            board.PinMask = board.WhiteKingPinnedRay();
            if (board.NumCheckers == 1)
            {
                board.CaptureMask = board.Checkers;
                board.PushMask = *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(board.Checkers));
            }

            var oldNodes = summary.Nodes;

            var positions = board.WhitePawn;
            while (positions != 0) PerftWhitePawn(ref board, ref summary, depth, positions.PopLSB());

            positions = board.WhiteKnight;
            while (positions != 0) PerftWhiteKnight(ref board, ref summary, depth, positions.PopLSB());

            positions = board.WhiteBishop;
            while (positions != 0) PerftWhiteBishop(ref board, ref summary, depth, positions.PopLSB());

            positions = board.WhiteRook;
            while (positions != 0) PerftWhiteRook(ref board, ref summary, depth, positions.PopLSB());

            positions = board.WhiteQueen;
            while (positions != 0) PerftWhiteQueen(ref board, ref summary, depth, positions.PopLSB());

            positions = board.WhiteKing;
            while (positions != 0) PerftWhiteKing(ref board, ref summary, depth, positions.PopLSB());

            if (oldNodes == summary.Nodes) summary.CheckMates++;
        }
        else
        {
            if (depth == 0)
            {
                summary.Nodes++;
                return;
            }
            board.Checkers = board.WhiteCheckers();
            board.NumCheckers = ulong.PopCount(board.Checkers);
            board.AttackedSquares = board.BlackKingDangerSquares();
            board.CaptureMask = 0xFFFFFFFFFFFFFFFF;
            board.PushMask = 0xFFFFFFFFFFFFFFFF;
            board.PinMask = board.BlackKingPinnedRay();
            if (board.NumCheckers == 1)
            {
                board.CaptureMask = board.Checkers;
                board.PushMask = *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(board.Checkers));
            }

            var oldNodes = summary.Nodes;
            var positions = board.BlackPawn;
            while (positions != 0) PerftBlackPawn(ref board, ref summary, depth, positions.PopLSB());

            positions = board.BlackKnight;
            while (positions != 0) PerftBlackKnight(ref board, ref summary, depth, positions.PopLSB());

            positions = board.BlackBishop;
            while (positions != 0) PerftBlackBishop(ref board, ref summary, depth, positions.PopLSB());

            positions = board.BlackRook;
            while (positions != 0) PerftBlackRook(ref board, ref summary, depth, positions.PopLSB());

            positions = board.BlackQueen;
            while (positions != 0) PerftBlackQueen(ref board, ref summary, depth, positions.PopLSB());

            positions = board.BlackKing;
            while (positions != 0) PerftBlackKing(ref board, ref summary, depth, positions.PopLSB());

            if (depth == 1 && oldNodes == summary.Nodes) summary.CheckMates++;
        }
    }

    public static void PerftWhite(ref Board board, ref Summary summary, int depth)
    {
        board.Checkers = board.BlackCheckers();
        board.NumCheckers = ulong.PopCount(board.Checkers);

        if (depth == 0)
        {
            if (board.NumCheckers == 1)
            {
                board.CaptureMask = 0xFFFFFFFFFFFFFFFF;
                board.PushMask = 0xFFFFFFFFFFFFFFFF;
                board.PinMask = board.WhiteKingPinnedRay();
                if (board.NumCheckers == 1)
                {
                    board.CaptureMask = board.Checkers;
                    board.PushMask = *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(board.Checkers));
                }
                board.AttackedSquares = board.WhiteKingDangerSquares();

                summary.AddCheck();
                MateChecker.WhiteSingleCheckEvasionTest(ref board, ref summary);
            }
            else if (board.NumCheckers > 1)
            {
                summary.AddDoubleCheck();
                board.AttackedSquares = board.WhiteKingDangerSquares();
                MateChecker.WhiteDoubleCheckEvasionTest(ref board, ref summary);
            }

            summary.Nodes++;
            return;
        }

        board.AttackedSquares = board.WhiteKingDangerSquares();

        var positions = board.WhiteKing;
        while (positions != 0) PerftWhiteKing(ref board, ref summary, depth, positions.PopLSB());

        if (board.NumCheckers > 1)
        {
            // Only a king move can evade double check
            return;
        }

        board.CaptureMask = 0xFFFFFFFFFFFFFFFF;
        board.PushMask = 0xFFFFFFFFFFFFFFFF;
        board.PinMask = board.WhiteKingPinnedRay();
        if (board.NumCheckers == 1)
        {
            board.CaptureMask = board.Checkers;
            board.PushMask = *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(board.Checkers));
        }

        positions = board.WhitePawn;
        while (positions != 0) PerftWhitePawn(ref board, ref summary, depth, positions.PopLSB());

        positions = board.WhiteKnight;
        while (positions != 0) PerftWhiteKnight(ref board, ref summary, depth, positions.PopLSB());

        positions = board.WhiteBishop;
        while (positions != 0) PerftWhiteBishop(ref board, ref summary, depth, positions.PopLSB());

        positions = board.WhiteRook;
        while (positions != 0) PerftWhiteRook(ref board, ref summary, depth, positions.PopLSB());

        positions = board.WhiteQueen;
        while (positions != 0) PerftWhiteQueen(ref board, ref summary, depth, positions.PopLSB());


    }

    public static void PerftBlack(ref Board board, ref Summary summary, int depth)
    {
        board.Checkers = board.WhiteCheckers();
        board.NumCheckers = ulong.PopCount(board.Checkers);

        if (depth == 0)
        {
            if (board.NumCheckers == 1)
            {
                board.CaptureMask = 0xFFFFFFFFFFFFFFFF;
                board.PushMask = 0xFFFFFFFFFFFFFFFF;
                board.PinMask = board.BlackKingPinnedRay();

                if (board.NumCheckers == 1)
                {
                    board.CaptureMask = board.Checkers;
                    board.PushMask = *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(board.Checkers));
                }
                board.AttackedSquares = board.BlackKingDangerSquares();


                summary.AddCheck();
                MateChecker.BlackSingleCheckEvasionTest(ref board, ref summary);
            }
            else if (board.NumCheckers > 1)
            {
                summary.AddDoubleCheck();
                board.AttackedSquares = board.BlackKingDangerSquares();
                MateChecker.BlackDoubleCheckEvasionTest(ref board, ref summary);
            }

            summary.Nodes++;
            return;
        }

        board.AttackedSquares = board.BlackKingDangerSquares();

        var positions = board.BlackKing;
        while (positions != 0) PerftBlackKing(ref board, ref summary, depth, positions.PopLSB());

        if (board.NumCheckers > 1)
        {
            // Only a king move can evade double check
            return;
        }

        board.CaptureMask = 0xFFFFFFFFFFFFFFFF;
        board.PushMask = 0xFFFFFFFFFFFFFFFF;
        board.PinMask = board.BlackKingPinnedRay();

        if (board.NumCheckers == 1)
        {
            board.CaptureMask = board.Checkers;
            board.PushMask = *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(board.Checkers));
        }

        positions = board.BlackPawn;
        while (positions != 0) PerftBlackPawn(ref board, ref summary, depth, positions.PopLSB());

        positions = board.BlackKnight;
        while (positions != 0) PerftBlackKnight(ref board, ref summary, depth, positions.PopLSB());

        positions = board.BlackBishop;
        while (positions != 0) PerftBlackBishop(ref board, ref summary, depth, positions.PopLSB());

        positions = board.BlackRook;
        while (positions != 0) PerftBlackRook(ref board, ref summary, depth, positions.PopLSB());

        positions = board.BlackQueen;
        while (positions != 0) PerftBlackQueen(ref board, ref summary, depth, positions.PopLSB());
    }

    public static void PerftWhitePawn(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var rankIndex = index.GetRankIndex();
        var posEncoded = 1UL << index;

        if (board.EnPassantFile != 8 && rankIndex.IsWhiteEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_Enpassant(index);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
            {
                if (depth == 1) summary.AddEnpassant();
                PerftBlack(ref newBoard, ref summary, depth - 1);
            }
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
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }
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
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddCapture();
                    PerftBlack(ref newBoard, ref summary, depth - 1);
                }
            }
        }

        // Move up
        target = posEncoded.ShiftUp();
        if ((board.Occupancy & target) > 0)
            // Blocked from moving down
            return;

        toSquare = index.ShiftUp();
        if (canPromote)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_KnightPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftBlack(ref newBoard, ref summary, depth - 1);
            }

            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_BishopPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftBlack(ref newBoard, ref summary, depth - 1);
            }

            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_RookPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftBlack(ref newBoard, ref summary, depth - 1);
            }

            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_QueenPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftBlack(ref newBoard, ref summary, depth - 1);
            }

            return;
        }

        board.CloneTo(ref newBoard);
        newBoard.WhitePawn_Move(index, toSquare);
        if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) PerftBlack(ref newBoard, ref summary, depth - 1);
        target = target.ShiftUp();
        if (rankIndex.IsSecondRank() && (board.Occupancy & target) == 0)
        {
            // Double push
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_DoublePush(index, toSquare.ShiftUp());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos)) PerftBlack(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftWhiteKnight(ref Board board, ref Summary summary, int depth, int index)
    {
        if ((board.PinMask & (1ul << index)) != 0)
        {
            return;
        }

        Board newBoard = default;
        var potentialMoves = *(AttackTables.KnightAttackTable + index) & (board.PushMask | board.CaptureMask);
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKnight_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKnight_Move(index, emptyMoves.PopLSB());
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftWhiteBishop(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
        }

        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteBishop_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteBishop_Move(index, emptyMoves.PopLSB());
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftWhiteRook(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }

        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteRook_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteRook_Move(index, emptyMoves.PopLSB());
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftWhiteQueen(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index) | AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }

        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteQueen_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteQueen_Move(index, emptyMoves.PopLSB());
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftWhiteKing(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KingAttackTable + index) & ~board.AttackedSquares;

        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_Move(index, emptyMoves.PopLSB());
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }

        if (index != 4 || board.NumCheckers > 0)
            // Can't castle if king is attacked or not on the starting position
            return;

        if ((board.CastleRights & CastleRights.WhiteKingSide) != 0 &&
            (board.WhiteRook & WhiteKingSideCastleRookPosition) > 0 &&
            (board.Occupancy & WhiteKingSideCastleEmptyPositions) == 0 &&
            (board.AttackedSquares & (1ul << 6)) == 0 &&
            (board.AttackedSquares & (1ul << 5)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_KingSideCastle();
            if (depth == 1) summary.AddCastle();
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }

        // Queen Side Castle
        if ((board.CastleRights & CastleRights.WhiteQueenSide) != 0 &&
            (board.WhiteRook & WhiteQueenSideCastleRookPosition) > 0 &&
            (board.Occupancy & WhiteQueenSideCastleEmptyPositions) == 0 &&
              (board.AttackedSquares & (1ul << 2)) == 0 &&
            (board.AttackedSquares & (1ul << 3)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_QueenSideCastle();
            if (depth == 1) summary.AddCastle();
            PerftBlack(ref newBoard, ref summary, depth - 1);
        }
    }


    public static void PerftBlackPawn(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var rankIndex = index.GetRankIndex();
        var posEncoded = 1UL << index;

        if (board.EnPassantFile != 8 && rankIndex.IsBlackEnPassantRankIndex() &&
            Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_Enpassant(index);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
            {
                if (depth == 1) summary.AddEnpassant();
                PerftWhite(ref newBoard, ref summary, depth - 1);
            }
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
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }
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
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }

                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddPromotionCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddCapture();
                    PerftWhite(ref newBoard, ref summary, depth - 1);
                }
            }
        }

        // Vertical moves
        target = posEncoded.ShiftDown();
        if ((board.Occupancy & target) > 0)
            // Blocked from moving down
            return;

        toSquare = index.ShiftDown();
        if (canPromote)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_KnightPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftWhite(ref newBoard, ref summary, depth - 1);
            }

            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_BishopPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftWhite(ref newBoard, ref summary, depth - 1);
            }

            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_RookPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftWhite(ref newBoard, ref summary, depth - 1);
            }

            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_QueenPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
            {
                if (depth == 1) summary.AddPromotion();
                PerftWhite(ref newBoard, ref summary, depth - 1);
            }

            return;
        }

        // Move down
        board.CloneTo(ref newBoard);
        newBoard.BlackPawn_Move(index, toSquare);
        if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) PerftWhite(ref newBoard, ref summary, depth - 1);
        target = target.ShiftDown();
        if (rankIndex.IsSeventhRank() && (board.Occupancy & target) == 0)
        {
            // Double push
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_DoublePush(index, toSquare.ShiftDown());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos)) PerftWhite(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftBlackKnight(ref Board board, ref Summary summary, int depth, int index)
    {
        if ((board.PinMask & (1ul << index)) != 0)
        {
            return;
        }

        Board newBoard = default;

        var potentialMoves = *(AttackTables.KnightAttackTable + index) & (board.PushMask | board.CaptureMask);
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKnight_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKnight_Move(index, emptyMoves.PopLSB());
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftBlackBishop(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
        }

        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackBishop_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackBishop_Move(index, emptyMoves.PopLSB());
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftBlackRook(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }

        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackRook_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackRook_Move(index, emptyMoves.PopLSB());
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftBlackQueen(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & (board.PushMask | board.CaptureMask);

        if ((board.PinMask & (1ul << index)) != 0)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index) | AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }

        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackQueen_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackQueen_Move(index, emptyMoves.PopLSB());
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }
    }

    public static void PerftBlackKing(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KingAttackTable + index) & ~board.AttackedSquares;
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_Capture(index, captureMoves.PopLSB());
            if (depth == 1) summary.AddCapture();
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_Move(index, emptyMoves.PopLSB());
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }

        if (index != 60 || board.NumCheckers > 0)
            // Can't castle if king is attacked or not on the starting position
            return;

        // King Side Castle
        if ((board.CastleRights & CastleRights.BlackKingSide) != 0 &&
            (board.BlackRook & BlackKingSideCastleRookPosition) > 0 &&
            (board.Occupancy & BlackKingSideCastleEmptyPositions) == 0 &&
            (board.AttackedSquares & (1ul << 61)) == 0 &&
            (board.AttackedSquares & (1ul << 62)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_KingSideCastle();
            if (depth == 1) summary.AddCastle();
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }

        // Queen Side Castle
        if ((board.CastleRights & CastleRights.BlackQueenSide) != 0 &&
            (board.BlackRook & BlackQueenSideCastleRookPosition) > 0 &&
            (board.Occupancy & BlackQueenSideCastleEmptyPositions) == 0 &&
            (board.AttackedSquares & (1ul << 58)) == 0 &&
            (board.AttackedSquares & (1ul << 59)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_QueenSideCastle();
            if (depth == 1) summary.AddCastle();
            PerftWhite(ref newBoard, ref summary, depth - 1);
        }
    }
}