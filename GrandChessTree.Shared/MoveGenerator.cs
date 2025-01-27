using GrandChessTree.Client.Tables;

namespace GrandChessTree.Client;

public static unsafe class MoveGenerator
{
    public const ulong BlackKingSideCastleRookPosition = 1UL << 63;
    public const ulong BlackKingSideCastleEmptyPositions = (1UL << 61) | (1UL << 62);
    public const ulong BlackQueenSideCastleRookPosition = 1UL << 56;
    public const ulong BlackQueenSideCastleEmptyPositions = (1UL << 57) | (1UL << 58) | (1UL << 59);

    public const ulong WhiteKingSideCastleRookPosition = 1UL << 7;
    public const ulong WhiteKingSideCastleEmptyPositions = (1UL << 6) | (1UL << 5);
    public const ulong WhiteQueenSideCastleRookPosition = 1UL;
    public const ulong WhiteQueenSideCastleEmptyPositions = (1UL << 1) | (1UL << 2) | (1UL << 3);

    public static Board[] PerftRoot(ref Board board, int depth, bool whiteToMove)
    {
        var summary = Perft.PerftRoot(ref board, depth, whiteToMove);

        var output = new Board[summary.Nodes];
        var boards = new Span<Board>(output);
        var moveIndex = 0;

        if (whiteToMove)
        {
            var positions = board.WhitePawn;
            while (positions != 0) PerftWhitePawn(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.WhiteKnight;
            while (positions != 0) PerftWhiteKnight(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.WhiteBishop;
            while (positions != 0) PerftWhiteBishop(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.WhiteRook;
            while (positions != 0) PerftWhiteRook(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.WhiteQueen;
            while (positions != 0) PerftWhiteQueen(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.WhiteKing;
            while (positions != 0) PerftWhiteKing(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());
        }
        else
        {
            var positions = board.BlackPawn;
            while (positions != 0) PerftBlackPawn(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.BlackKnight;
            while (positions != 0) PerftBlackKnight(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.BlackBishop;
            while (positions != 0) PerftBlackBishop(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.BlackRook;
            while (positions != 0) PerftBlackRook(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.BlackQueen;
            while (positions != 0) PerftBlackQueen(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

            positions = board.BlackKing;
            while (positions != 0) PerftBlackKing(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());
        }

        var leafNodeWhiteToMove = depth % 2 == 0 ? whiteToMove : !whiteToMove;
        for(int i = 0; i < boards.Length; i++)
        {
            output[i].Hash = Zobrist.CalculateZobristKey(ref output[i], leafNodeWhiteToMove);
        }

        return output;
    }

    public static void PerftWhite(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth)
    {
        if (depth == 0)
        {
            boards[moveIndex++] = board;
            return;
        }

        var positions = board.WhitePawn;
        while (positions != 0) PerftWhitePawn(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.WhiteKnight;
        while (positions != 0) PerftWhiteKnight(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.WhiteBishop;
        while (positions != 0) PerftWhiteBishop(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.WhiteRook;
        while (positions != 0) PerftWhiteRook(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.WhiteQueen;
        while (positions != 0) PerftWhiteQueen(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.WhiteKing;
        while (positions != 0) PerftWhiteKing(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());
    }

    public static void PerftBlack(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth)
    {
        if (depth == 0)
        {
            boards[moveIndex++] = board;
            return;
        }

        var positions = board.BlackPawn;
        while (positions != 0) PerftBlackPawn(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.BlackKnight;
        while (positions != 0) PerftBlackKnight(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.BlackBishop;
        while (positions != 0) PerftBlackBishop(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.BlackRook;
        while (positions != 0) PerftBlackRook(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.BlackQueen;
        while (positions != 0) PerftBlackQueen(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());

        positions = board.BlackKing;
        while (positions != 0) PerftBlackKing(ref board, ref boards, ref moveIndex, depth, positions.PopLSB());
    }

    public static void PerftWhitePawn(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
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
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
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
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
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
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                    PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
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
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_BishopPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_RookPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_QueenPromotion(index, toSquare);
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
            return;
        }

        board.CloneTo(ref newBoard);
        newBoard.WhitePawn_Move(index, toSquare);
        if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
            PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        target = target.ShiftUp();
        if (rankIndex.IsSecondRank() && (board.Occupancy & target) == 0)
        {
            // Double push
            board.CloneTo(ref newBoard);
            newBoard.WhitePawn_DoublePush(index, toSquare.ShiftUp());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftWhiteKnight(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth,
        int index)
    {
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KnightAttackTable + index);
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKnight_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKnight_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftWhiteBishop(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth,
        int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index);
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteBishop_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteBishop_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftWhiteRook(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index);
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteRook_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteRook_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftWhiteQueen(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index);
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteQueen_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteQueen_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftWhiteKing(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KingAttackTable + index);

        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        if (index != 4 || board.IsAttackedByBlack(board.WhiteKingPos))
            // Can't castle if king is attacked or not on the starting position
            return;

        if ((board.CastleRights & CastleRights.WhiteKingSide) != 0 &&
            (board.WhiteRook & WhiteKingSideCastleRookPosition) > 0 &&
            (board.Occupancy & WhiteKingSideCastleEmptyPositions) == 0 &&
            !board.IsAttackedByBlack(6) &&
            !board.IsAttackedByBlack(5))
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_KingSideCastle();
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        // Queen Side Castle
        if ((board.CastleRights & CastleRights.WhiteQueenSide) != 0 &&
            (board.WhiteRook & WhiteQueenSideCastleRookPosition) > 0 &&
            (board.Occupancy & WhiteQueenSideCastleEmptyPositions) == 0 &&
            !board.IsAttackedByBlack(2) &&
            !board.IsAttackedByBlack(3))
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_QueenSideCastle();
            if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                PerftBlack(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }


    public static void PerftBlackPawn(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
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
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
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
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
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
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
            }
            else
            {
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                    PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
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
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_BishopPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_RookPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_QueenPromotion(index, toSquare);
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
            return;
        }

        // Move down
        board.CloneTo(ref newBoard);
        newBoard.BlackPawn_Move(index, toSquare);
        if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
            PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        target = target.ShiftDown();
        if (rankIndex.IsSeventhRank() && (board.Occupancy & target) == 0)
        {
            // Double push
            board.CloneTo(ref newBoard);
            newBoard.BlackPawn_DoublePush(index, toSquare.ShiftDown());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftBlackKnight(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth,
        int index)
    {
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KnightAttackTable + index);
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKnight_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKnight_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftBlackBishop(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth,
        int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index);
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackBishop_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackBishop_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftBlackRook(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index);
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackRook_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackRook_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftBlackQueen(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index);
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackQueen_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackQueen_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }

    public static void PerftBlackKing(ref Board board, ref Span<Board> boards, ref int moveIndex, int depth, int index)
    {
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KingAttackTable + index);
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_Capture(index, captureMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_Move(index, emptyMoves.PopLSB());
            if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        if (index != 60 || board.IsAttackedByWhite(board.BlackKingPos))
            // Can't castle if king is attacked or not on the starting position
            return;

        // King Side Castle
        if ((board.CastleRights & CastleRights.BlackKingSide) != 0 &&
            (board.BlackRook & BlackKingSideCastleRookPosition) > 0 &&
            (board.Occupancy & BlackKingSideCastleEmptyPositions) == 0 &&
            !board.IsAttackedByWhite(61) &&
            !board.IsAttackedByWhite(62))
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_KingSideCastle();
            if (!newBoard.IsAttackedByWhite(62)) PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }

        // Queen Side Castle
        if ((board.CastleRights & CastleRights.BlackQueenSide) != 0 &&
            (board.BlackRook & BlackQueenSideCastleRookPosition) > 0 &&
            (board.Occupancy & BlackQueenSideCastleEmptyPositions) == 0 &&
            !board.IsAttackedByWhite(58) &&
            !board.IsAttackedByWhite(59))
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_QueenSideCastle();
            if (!newBoard.IsAttackedByWhite(58)) PerftWhite(ref newBoard, ref boards, ref moveIndex, depth - 1);
        }
    }
}