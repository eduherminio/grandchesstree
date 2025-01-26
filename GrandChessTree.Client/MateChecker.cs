using Sapling.Engine.MoveGen;

namespace GreatPerft
{
    public static unsafe class MateChecker
    {
        public static void WhiteCheckTest(ref Board board, ref Summary summary)
        {
            if (AttackTables.IsAttackedByBlack(ref board, board.WhiteKingPos))
            {
                summary.AddCheck();
            }
            else
            {
                return;
            }

            var oldNodes = summary.Nodes;

            var positions = board.WhitePawn;
            while (positions != 0)
            {
                if(PerftWhitePawn(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.WhiteKnight;
            while (positions != 0)
            {
                if(PerftWhiteKnight(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.WhiteBishop;
            while (positions != 0)
            {
                if(PerftWhiteBishop(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.WhiteRook;
            while (positions != 0)
            {
                if(PerftWhiteRook(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.WhiteQueen;
            while (positions != 0)
            {
                if(PerftWhiteQueen(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.WhiteKing;
            while (positions != 0)
            {
                if(PerftWhiteKing(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            summary.AddMate();
        }

        public static void BlackCheckTest(ref Board board, ref Summary summary)
        {
            if (AttackTables.IsAttackedByWhite(ref board, board.BlackKingPos))
            {
                summary.AddCheck();
            }
            else
            {
                return;
            }


            var oldNodes = summary.Nodes;
            var positions = board.BlackPawn;
            while (positions != 0)
            {
                if(PerftBlackPawn(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.BlackKnight;
            while (positions != 0)
            {
                if(PerftBlackKnight(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.BlackBishop;
            while (positions != 0)
            {
                if(PerftBlackBishop(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.BlackRook;
            while (positions != 0)
            {
                if(PerftBlackRook(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.BlackQueen;
            while (positions != 0)
            {
                if(PerftBlackQueen(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            positions = board.BlackKing;
            while (positions != 0)
            {
                if(PerftBlackKing(ref board, positions.PopLSB()))
                {
                    return;
                }
            }

            summary.AddMate();
            return;
        }

        public static bool PerftWhitePawn(ref Board board, int index)
        {
            Board newBoard = default;

            var rankIndex = index.GetRankIndex();
            var posEncoded = 1UL << index;

            if (board.EnPassantFile != 8 && rankIndex.IsWhiteEnPassantRankIndex() &&
                Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhitePawn_Enpassant(index);
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
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
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_KnightPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                }
                else
                {
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
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
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_KnightPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                }
                else
                {
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.WhitePawn_Capture(index, toSquare);
                    if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                    {
                        return true;
                    }
                }
            }

            // Move up
            target = posEncoded.ShiftUp();
            if ((board.Occupancy & target) > 0)
            {
                // Blocked from moving down
                return false;
            }

            toSquare = index.ShiftUp();
            if (canPromote)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhitePawn_KnightPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhitePawn_BishopPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhitePawn_RookPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhitePawn_QueenPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }

                return false;
            }

            BoardExtensions.CloneTo(ref board, ref newBoard);
            newBoard.WhitePawn_Move(index, toSquare);
            if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
            {
                return true;
            }
            target = target.ShiftUp();
            if (rankIndex.IsSecondRank() && (board.Occupancy & target) == 0)
            {
                // Double push
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhitePawn_DoublePush(index, toSquare.ShiftUp());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PerftWhiteKnight(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = *(AttackTables.KnightAttackTable + index);
            var captureMoves = potentialMoves & board.Black;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteKnight_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteKnight_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PerftWhiteBishop(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index);
            var captureMoves = potentialMoves & board.Black;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteBishop_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteBishop_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PerftWhiteRook(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index);
            var captureMoves = potentialMoves & board.Black;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteRook_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteRook_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PerftWhiteQueen(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) |
                            AttackTables.PextRookAttacks(board.Occupancy, index);
            var captureMoves = potentialMoves & board.Black;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteQueen_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteQueen_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            return false;
        }
        public const ulong BlackKingSideCastleRookPosition = 1UL << 63;
        public const ulong BlackKingSideCastleEmptyPositions = (1UL << 61) | (1UL << 62);
        public const ulong BlackQueenSideCastleRookPosition = 1UL << 56;
        public const ulong BlackQueenSideCastleEmptyPositions = (1UL << 57) | (1UL << 58) | (1UL << 59);

        public const ulong WhiteKingSideCastleRookPosition = 1UL << 7;
        public const ulong WhiteKingSideCastleEmptyPositions = (1UL << 6) | (1UL << 5);
        public const ulong WhiteQueenSideCastleRookPosition = 1UL;
        public const ulong WhiteQueenSideCastleEmptyPositions = (1UL << 1) | (1UL << 2) | (1UL << 3);

        public static bool PerftWhiteKing(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = *(AttackTables.KingAttackTable + index);

            var captureMoves = potentialMoves & board.Black;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteKing_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteKing_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            if (index != 4 || AttackTables.IsAttackedByBlack(ref board, board.WhiteKingPos))
            {
                // Can't castle if king is attacked or not on the starting position
                return false;
            }

            if ((board.CastleRights & CastleRights.WhiteKingSide) != 0 &&
            (board.WhiteRook & WhiteKingSideCastleRookPosition) > 0 &&
            (board.Occupancy & WhiteKingSideCastleEmptyPositions) == 0 &&
            !board.IsAttackedByBlack(6) &&
            !board.IsAttackedByBlack(5))
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteKing_KingSideCastle();
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            // Queen Side Castle
            if ((board.CastleRights & CastleRights.WhiteQueenSide) != 0 &&
                (board.WhiteRook & WhiteQueenSideCastleRookPosition) > 0 &&
                (board.Occupancy & WhiteQueenSideCastleEmptyPositions) == 0 &&
                !board.IsAttackedByBlack(2) &&
                !board.IsAttackedByBlack(3))
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.WhiteKing_QueenSideCastle();
                if (!AttackTables.IsAttackedByBlack(ref newBoard, newBoard.WhiteKingPos))
                {
                    return true;
                }
            }

            return false;
        }
 

        public static bool PerftBlackPawn(ref Board board, int index)
        {
            Board newBoard = default;

            var rankIndex = index.GetRankIndex();
            var posEncoded = 1UL << index;

            if (board.EnPassantFile != 8 && rankIndex.IsBlackEnPassantRankIndex() &&
                Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackPawn_Enpassant(index);
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;
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
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_KnightPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;
                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                }
                else
                {
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

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
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_KnightPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                }
                else
                {
                    BoardExtensions.CloneTo(ref board, ref newBoard);
                    newBoard.BlackPawn_Capture(index, toSquare);
                    if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                    {
                        return true;

                    }
                }
            }

            // Vertical moves
            target = posEncoded.ShiftDown();
            if ((board.Occupancy & target) > 0)
            {
                // Blocked from moving down
                return false;
            }

            toSquare = index.ShiftDown();
            if (canPromote)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackPawn_KnightPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackPawn_BishopPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackPawn_RookPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackPawn_QueenPromotion(index, toSquare);
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
                return false;
            }

            // Move down
            BoardExtensions.CloneTo(ref board, ref newBoard);
            newBoard.BlackPawn_Move(index, toSquare);
            if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
            {
                return true;
            }
            target = target.ShiftDown();
            if (rankIndex.IsSeventhRank() && (board.Occupancy & target) == 0)
            {
                // Double push
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackPawn_DoublePush(index, toSquare.ShiftDown());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PerftBlackKnight(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = *(AttackTables.KnightAttackTable + index);
            var captureMoves = potentialMoves & board.White;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackKnight_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackKnight_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PerftBlackBishop(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index);
            var captureMoves = potentialMoves & board.White;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackBishop_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackBishop_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
            }

            return false;
        }

        public static bool PerftBlackRook(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index);
            var captureMoves = potentialMoves & board.White;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackRook_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackRook_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PerftBlackQueen(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) |
                           AttackTables.PextRookAttacks(board.Occupancy, index);
            var captureMoves = potentialMoves & board.White;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackQueen_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackQueen_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
            }

            return false;
        }

        public static bool PerftBlackKing(ref Board board, int index)
        {
            Board newBoard = default;

            var potentialMoves = *(AttackTables.KingAttackTable + index);
            var captureMoves = potentialMoves & board.White;
            while (captureMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackKing_Capture(index, captureMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;

                }
            }

            var emptyMoves = potentialMoves & ~board.Occupancy;
            while (emptyMoves != 0)
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackKing_Move(index, emptyMoves.PopLSB());
                if (!AttackTables.IsAttackedByWhite(ref newBoard, newBoard.BlackKingPos))
                {
                    return true;
                }
            }

            if (index != 60 || AttackTables.IsAttackedByWhite(ref board, board.BlackKingPos))
            {
                // Can't castle if king is attacked or not on the starting position
                return false;
            }

            // King Side Castle
            if ((board.CastleRights & CastleRights.BlackKingSide) != 0 &&
                (board.BlackRook & BlackKingSideCastleRookPosition) > 0 &&
                (board.Occupancy & BlackKingSideCastleEmptyPositions) == 0 &&
                !board.IsAttackedByWhite(61) &&
                !board.IsAttackedByWhite(62))
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackKing_KingSideCastle();
                if (!AttackTables.IsAttackedByWhite(ref newBoard, 62))
                {
                    return true;

                }
            }

            // Queen Side Castle
            if ((board.CastleRights & CastleRights.BlackQueenSide) != 0 &&
                (board.BlackRook & BlackQueenSideCastleRookPosition) > 0 &&
                (board.Occupancy & BlackQueenSideCastleEmptyPositions) == 0 &&
                !board.IsAttackedByWhite(58) &&
                !board.IsAttackedByWhite(59))
            {
                BoardExtensions.CloneTo(ref board, ref newBoard);
                newBoard.BlackKing_QueenSideCastle();
                if (!AttackTables.IsAttackedByWhite(ref newBoard, 58))
                {
                    return true;

                }
            }

            return false;
        }
    }
}
