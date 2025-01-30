using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;

namespace GrandChessTree.Shared;
public partial struct Board
{
    public unsafe void AccumulateBlackMoves(ref Summary summary, int depth, int prevDestination)
    {
        var checkers = WhiteCheckers();
        var numCheckers = (byte)ulong.PopCount(checkers);
        if (depth == 0)
        {
            if (numCheckers == 1)
            {
                summary.Nodes++;
                MoveMask = checkers | *(AttackTables.LineBitBoardsInclusive + BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(checkers));
                var isMate = !BlackCanEvadeSingleCheck();
                var isDiscovered = (checkers & (1UL << prevDestination)) == 0;
                if (isMate)
                {
                    if (isDiscovered)
                    {
                        summary.SingleDiscoveredCheckmate++;
                    }
                    else
                    {
                        summary.DirectCheckmate++;
                    }
                }
                else
                {
                    if (isDiscovered)
                    {
                        summary.SingleDiscoveredCheck++;
                    }
                    else
                    {
                        summary.DirectCheck++;
                    }
                }

                return;
            }

            if (numCheckers > 1)
            {
                summary.Nodes++;
                var isMate = !CanBlackKingMove();
                var isDiscovered = (checkers & (1UL << prevDestination)) == 0;
                if (isMate)
                {
                    if (isDiscovered)
                    {
                        summary.DoubleDiscoverdCheckmate++;
                    }
                    else
                    {
                        summary.DirectDiscoverdCheckmate++;
                    }
                }
                else
                {
                    if (isDiscovered)
                    {
                        summary.DoubleDiscoveredCheck++;
                    }
                    else
                    {
                        summary.DirectDiscoveredCheck++;
                    }
                }

                return;
            }

            summary.Nodes++;
            return;
        }
    
        var ptr = (Perft.HashTable + (Hash & Perft.HashTableMask));
        var hashEntry = Unsafe.Read<Summary>(ptr);
        if (hashEntry.FullHash == (Hash ^  Occupancy) && depth == hashEntry.Depth)
        {
            summary.Accumulate(ref hashEntry);
            return;
        }

        hashEntry = default;
        hashEntry.FullHash = Hash ^  Occupancy;
        hashEntry.Depth = (byte)depth;
        AccumulateBlackKingMoves(ref hashEntry, depth, BlackKingPos, numCheckers > 0);

        if (numCheckers > 1)
        {
            // Only a king move can evade double check
            summary.Accumulate(ref hashEntry);
            *ptr = hashEntry;
            return;
        }

        MoveMask = 0xFFFFFFFFFFFFFFFF;
        if (numCheckers == 1)
        {
            MoveMask = checkers | *(AttackTables.LineBitBoardsInclusive + BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(checkers));
        }
        var pinMask = BlackKingPinnedRay();

        var positions = BlackPawn;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackPawnMoves( ref hashEntry, depth, index, (pinMask & (1ul << index)) != 0);
        }

        positions = BlackKnight;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            if((pinMask & (1ul << index)) != 0)
            {
                // Pinned knight can't move
                continue;
            }

            AccumulateBlackKnightMoves( ref hashEntry, depth, index);
        }
        
        positions = BlackBishop;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackBishopMoves(ref hashEntry, depth, index, (pinMask & (1ul << index)) != 0);
        }
        
        positions = BlackRook;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackRookMoves( ref hashEntry, depth, index, (pinMask & (1ul << index)) != 0);
        }
        
        positions = BlackQueen;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackQueenMoves(ref hashEntry, depth, index, (pinMask & (1ul << index)) != 0);
        }
        
        summary.Accumulate(ref hashEntry);
        *ptr = hashEntry;
    }
    public unsafe void AccumulateBlackPawnMoves(ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;
        var rankIndex = SquareHelpers.GetRankIndex(index);
        int toSquare;
        if (rankIndex.IsSecondRank())
        {
            // Promoting moves
            var validMoves = *(AttackTables.BlackPawnAttackTable + index) & MoveMask & White;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeDiagonal(BlackKingPos, index);
            }

            while (validMoves != 0)
            {
                if (depth == 1) summary.AddPromotionCapture();
                toSquare = validMoves.PopLSB();

                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_Capture_KnightPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
            }

            validMoves = AttackTables.BlackPawnPushTable[index] & MoveMask & ~Occupancy;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeStraight(BlackKingPos, index);
            }
            while (validMoves != 0)
            {
                if (depth == 1) summary.AddPromotion();

                toSquare = validMoves.PopLSB();
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_KnightPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_BishopPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_RookPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_QueenPromotion(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
            }
        }
        else
        {
            var validMoves = *(AttackTables.BlackPawnAttackTable + index) & MoveMask & White;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeDiagonal(BlackKingPos, index);
            }

            while (validMoves != 0)
            {
                if (depth == 1) summary.AddCapture();

                toSquare = validMoves.PopLSB();
                newBoard = Unsafe.As<Board, Board>(ref this);

                newBoard.BlackPawn_Capture(index, toSquare);
                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
            }

            if (EnPassantFile != 8 && rankIndex.IsBlackEnPassantRankIndex() &&
                Math.Abs(SquareHelpers.GetFileIndex(index) - EnPassantFile) == 1)
            {
                newBoard = Unsafe.As<Board, Board>(ref this);

                toSquare = Constants.BlackEnpassantOffset + EnPassantFile;

                newBoard.BlackPawn_Enpassant(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddEnpassant();
                    newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
                }
            }

            validMoves = AttackTables.BlackPawnPushTable[index] & MoveMask & ~Occupancy;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeStraight(BlackKingPos, index);
            }
            while (validMoves != 0)
            {
                toSquare = validMoves.PopLSB();
                newBoard = Unsafe.As<Board, Board>(ref this);


                if (rankIndex.IsSeventhRank() && SquareHelpers.GetRankIndex(toSquare) == 4)
                {
                    // Double push: Check intermediate square
                    var intermediateSquare = (index + toSquare) / 2; // Midpoint between start and destination
                    if ((Occupancy & (1UL << intermediateSquare)) != 0)
                    {
                        continue; // Intermediate square is blocked, skip this move
                    }
                    newBoard.BlackPawn_DoublePush(index, toSquare);
                }
                else
                {
                    // single push
                    newBoard.BlackPawn_Move(index, toSquare);
                }

                newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
            }
        }

    }

    public unsafe void AccumulateBlackKnightMoves(ref Summary summary, int depth, int index)
    {
        Board newBoard = default;
        int toSquare;

        var potentialMoves = *(AttackTables.KnightAttackTable + index) & MoveMask;
        var captureMoves = potentialMoves & White;
        while (captureMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = captureMoves.PopLSB();

            newBoard.BlackKnight_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~Occupancy;
        while (emptyMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = emptyMoves.PopLSB();

            newBoard.BlackKnight_Move(index, toSquare);
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }
    }

    public unsafe void AccumulateBlackBishopMoves(ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(Occupancy, index) & MoveMask;

        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(BlackKingPos, index);
        }

        int toSquare;

        var captureMoves = potentialMoves & White;
        while (captureMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = captureMoves.PopLSB();

            newBoard.BlackBishop_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~Occupancy;
        while (emptyMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = emptyMoves.PopLSB();

            newBoard.BlackBishop_Move(index, toSquare);
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }
    }

    public unsafe void AccumulateBlackRookMoves(ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextRookAttacks(Occupancy, index) & MoveMask;

        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(BlackKingPos, index);
        }
        int toSquare;

        var captureMoves = potentialMoves & White;
        while (captureMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = captureMoves.PopLSB();

            newBoard.BlackRook_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~Occupancy;
        while (emptyMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = emptyMoves.PopLSB();

            newBoard.BlackRook_Move(index, toSquare);
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }
    }

    public unsafe void AccumulateBlackQueenMoves(ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;

        var potentialMoves = (AttackTables.PextBishopAttacks(Occupancy, index) |
                             AttackTables.PextRookAttacks(Occupancy, index)) & MoveMask;

        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(BlackKingPos, index) | AttackTables.GetRayToEdgeStraight(BlackKingPos, index);
        }
        int toSquare;

        var captureMoves = potentialMoves & White;
        while (captureMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = captureMoves.PopLSB();

            newBoard.BlackQueen_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~Occupancy;
        while (emptyMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = emptyMoves.PopLSB();

            newBoard.BlackQueen_Move(index, toSquare);
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }
    }

    public unsafe void AccumulateBlackKingMoves(ref Summary summary, int depth, int index, bool inCheck)
    {
        var attackedSquares = this.BlackKingDangerSquares();
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KingAttackTable + index) & ~attackedSquares;
        int toSquare;

        var captureMoves = potentialMoves & White;
        while (captureMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = captureMoves.PopLSB();
            newBoard.BlackKing_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~Occupancy;
        while (emptyMoves != 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            toSquare = emptyMoves.PopLSB();

            newBoard.BlackKing_Move(index, toSquare);
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, toSquare);
        }

        if (index != 60 || inCheck)
            // Can't castle if king is attacked or not on the starting position
            return;

        // King Side Castle
        if ((CastleRights & CastleRights.BlackKingSide) != 0 &&
            (BlackRook & Constants.BlackKingSideCastleRookPosition) > 0 &&
            (Occupancy & Constants.BlackKingSideCastleEmptyPositions) == 0 &&
            (attackedSquares & (1ul << 61)) == 0 &&
            (attackedSquares & (1ul << 62)) == 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            newBoard.BlackKing_KingSideCastle();
            if (depth == 1) summary.AddCastle();
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, 61);
        }

        // Queen Side Castle
        if ((CastleRights & CastleRights.BlackQueenSide) != 0 &&
            (BlackRook & Constants.BlackQueenSideCastleRookPosition) > 0 &&
            (Occupancy & Constants.BlackQueenSideCastleEmptyPositions) == 0 &&
            (attackedSquares & (1ul << 58)) == 0 &&
            (attackedSquares & (1ul << 59)) == 0)
        {
            newBoard = Unsafe.As<Board, Board>(ref this);

            newBoard.BlackKing_QueenSideCastle();
            if (depth == 1) summary.AddCastle();
            newBoard.AccumulateWhiteMoves(ref summary, depth - 1, 59);
        }
    }
}