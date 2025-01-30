using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using GrandChessTree.Shared.Tables;

namespace GrandChessTree.Shared;

[Flags]
public enum LeafNodeFlags: byte
{
    None = 0,
    Double = 1 << 0,
    Mate = 1 << 1,
    Check = 1 << 2,
}

public static unsafe class Perft
{
    private const ulong BlackKingSideCastleRookPosition = 1UL << 63;
    private const ulong BlackKingSideCastleEmptyPositions = (1UL << 61) | (1UL << 62);
    private const ulong BlackQueenSideCastleRookPosition = 1UL << 56;
    private const ulong BlackQueenSideCastleEmptyPositions = (1UL << 57) | (1UL << 58) | (1UL << 59);

    private const ulong WhiteKingSideCastleRookPosition = 1UL << 7;
    private const ulong WhiteKingSideCastleEmptyPositions = (1UL << 6) | (1UL << 5);
    private const ulong WhiteQueenSideCastleRookPosition = 1UL;
    private const ulong WhiteQueenSideCastleEmptyPositions = (1UL << 1) | (1UL << 2) | (1UL << 3);

    #region HashTable
    private static readonly Summary* HashTable;
    private static readonly uint HashTableMask;   
   

    static Perft()
    {
        var hashTableSize = (int)CalculateHashTableEntries(1024);
        HashTable = AllocateHashTable((nuint)hashTableSize);
        HashTableMask = (uint)hashTableSize - 1;
    }
    private static uint CalculateHashTableEntries(int sizeInMb)
    {
        var transpositionCount = (ulong)sizeInMb * 1024ul * 1024ul / (ulong)sizeof(Summary);
        if (!BitOperations.IsPow2(transpositionCount))
        {
            transpositionCount = BitOperations.RoundUpToPowerOf2(transpositionCount) >> 1;
        }

        if (transpositionCount > int.MaxValue)
        {
            throw new ArgumentException("Hash table too large");
        }

        return (uint)transpositionCount;
    }

    private static Summary* AllocateHashTable(nuint items)
    {
        const nuint alignment = 64;

        var bytes = ((nuint)sizeof(Summary) * (nuint)items);
        var block = NativeMemory.AlignedAlloc(bytes, alignment);
        NativeMemory.Clear(block, bytes);

        return (Summary*)block;
    }
    public static void ClearTable()
    {
        Unsafe.InitBlock(HashTable, 0, (uint)(sizeof(Summary) * (HashTableMask + 1)));
    }

    #endregion


    public static Summary PerftRoot(ref Board board, int depth, bool whiteToMove)
    {
        Summary summary = default;
        summary.FullHash = board.Hash ^ board.Occupancy;
        summary.Depth = (byte)depth;
        if (depth == 0)
        {
            // perft(0) = 1
            summary.Nodes++;
            return summary;
        }

        if (whiteToMove)
        {
            var Checkers = board.BlackCheckers();
            var NumCheckers = (byte)ulong.PopCount(Checkers);
            board.MoveMask = 0xFFFFFFFFFFFFFFFF;
            if (NumCheckers == 1)
            {
                board.MoveMask = Checkers | *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(Checkers));
            }
            var PinMask = board.WhiteKingPinnedRay();

            var positions = board.WhitePawn;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateWhitePawnMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }

            positions = board.WhiteKnight;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                if ((PinMask & (1ul << index)) != 0)
                {
                    // Pinned knight can't move
                    continue;
                }
                AccumulateWhiteKnightMoves(ref board, ref summary, depth, index);
            }

            positions = board.WhiteBishop;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateWhiteBishopMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }

            positions = board.WhiteRook;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateWhiteRookMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }

            positions = board.WhiteQueen;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateWhiteQueenMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }

            AccumulateWhiteKingMoves(ref board, ref summary, depth, board.WhiteKingPos, NumCheckers > 0);
            return summary;
        }
        else
        {
            var Checkers = board.WhiteCheckers();
            var NumCheckers = (byte)ulong.PopCount(Checkers);
            board.MoveMask = 0xFFFFFFFFFFFFFFFF;
            if (NumCheckers == 1)
            {
                board.MoveMask = Checkers | *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(Checkers));
            }
            var PinMask = board.BlackKingPinnedRay();

            var positions = board.BlackPawn;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateBlackPawnMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }

            positions = board.BlackKnight;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                if ((PinMask & (1ul << index)) != 0)
                {
                    // Pinned knight can't move
                    continue;
                }

                AccumulateBlackKnightMoves(ref board, ref summary, depth, index);
            }

            positions = board.BlackBishop;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateBlackBishopMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }

            positions = board.BlackRook;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateBlackRookMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }

            positions = board.BlackQueen;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                AccumulateBlackQueenMoves(ref board, ref summary, depth, index, (PinMask & (1ul << index)) != 0);
            }


            AccumulateBlackKingMoves(ref board, ref summary, depth, board.BlackKingPos, NumCheckers > 0);
            return summary;
        }
    }

    private static void AccumulateWhiteMoves(ref Board board, ref Summary summary, int depth, int prevDestination)
    {
        var Checkers = board.BlackCheckers();
        var NumCheckers = (byte)ulong.PopCount(Checkers);

        if (depth == 0)
        {
            // Leaf node
            if (NumCheckers == 1)
            {
                board.MoveMask = Checkers | *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(Checkers));
                summary.Nodes++;
                if (!MateChecker.WhiteCanEvadeCheck(ref board))
                {
                    summary.AddMate();
                    return;
                }

                if ((Checkers & (1UL << prevDestination)) == 0)
                {
                    summary.AddDiscoveredCheck();
                }
                else
                {
                    summary.AddCheck();
                }

                return;
            }
            else if (NumCheckers > 1)
            {
                summary.Nodes++;
                if (!MateChecker.CanWhiteKingMove(ref board))
                {
                    summary.AddMate();
                    return;
                }

                if ((Checkers & (1UL << prevDestination)) == 0)
                {
                    summary.AddDoubleDiscoveredCheck();
                }
                else
                {
                    summary.AddDoubleCheck();
                }

                return;
            }

            summary.Nodes++;
            return;
        }

        var ptr = (HashTable + (board.Hash & HashTableMask));
        var hashEntry = *ptr;
        if (hashEntry.FullHash == (board.Hash ^  board.Occupancy) && depth == hashEntry.Depth)
        {
            summary.Accumulate(ref hashEntry);
            return;
        }

        hashEntry = default;
        hashEntry.FullHash = board.Hash ^ board.Occupancy;
        hashEntry.Depth = (byte)depth;
        
        AccumulateWhiteKingMoves(ref board, ref hashEntry, depth, board.WhiteKingPos, NumCheckers > 0);

        if (NumCheckers > 1)
        {
            // Only a king move can evade double check
            summary.Accumulate(ref hashEntry);
            *ptr = hashEntry;
            return;
        }

        board.MoveMask = 0xFFFFFFFFFFFFFFFF;
        if (NumCheckers == 1)
        {
            board.MoveMask = Checkers | *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(Checkers));
        }
        var PinMask = board.WhiteKingPinnedRay();

        var positions = board.WhitePawn;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateWhitePawnMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }

        positions = board.WhiteKnight;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            if ((PinMask & (1ul << index)) != 0)
            {
                // Pinned knight can't move
                continue;
            }
            AccumulateWhiteKnightMoves(ref board, ref hashEntry, depth, index);
        }

        positions = board.WhiteBishop;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateWhiteBishopMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }

        positions = board.WhiteRook;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateWhiteRookMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }

        positions = board.WhiteQueen;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateWhiteQueenMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }

        summary.Accumulate(ref hashEntry);
        *ptr = hashEntry;
    }

    public static ulong hashcnt = 0;
    private static void AccumulateBlackMoves(ref Board board, ref Summary summary, int depth, int prevDestination)
    {

        var Checkers = board.WhiteCheckers();
        var NumCheckers = (byte)ulong.PopCount(Checkers);
        if (depth == 0)
        {
            if (NumCheckers == 1)
            {
                board.MoveMask = Checkers | *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(Checkers));
                summary.Nodes++;
                if (!MateChecker.BlackCanEvadeSingleCheck(ref board))
                {
                    summary.AddMate();
                    return;
                }


                if ((Checkers & (1UL << prevDestination)) == 0)
                {
                    summary.AddDiscoveredCheck();
                }
                else
                {
                    summary.AddCheck();
                }
        
                return;
            }
            else if (NumCheckers > 1)
            {
                summary.Nodes++;

                if (!MateChecker.CanBlackKingMove(ref board))
                {
                    summary.AddMate();
                    return;
                }

                if ((Checkers & (1UL << prevDestination)) == 0)
                {
                    summary.AddDoubleDiscoveredCheck();
                }
                else
                {
                    summary.AddDoubleCheck();
                }
                
                return;
            }

            summary.Nodes++;
            return;
        }
    
        var ptr = (HashTable + (board.Hash & HashTableMask));
        var hashEntry = *ptr;
        if (hashEntry.FullHash == (board.Hash ^  board.Occupancy) && depth == hashEntry.Depth)
        {
            summary.Accumulate(ref hashEntry);
            return;
        }

        hashEntry = default;
        hashEntry.FullHash = board.Hash ^  board.Occupancy;
        hashEntry.Depth = (byte)depth;
        AccumulateBlackKingMoves(ref board, ref hashEntry, depth, board.BlackKingPos, NumCheckers > 0);

        if (NumCheckers > 1)
        {
            // Only a king move can evade double check
            summary.Accumulate(ref hashEntry);
            *ptr = hashEntry;
            return;
        }

        board.MoveMask = 0xFFFFFFFFFFFFFFFF;
        if (NumCheckers == 1)
        {
            board.MoveMask = Checkers | *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(Checkers));
        }
        var PinMask = board.BlackKingPinnedRay();

        var positions = board.BlackPawn;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackPawnMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }

        positions = board.BlackKnight;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            if((PinMask & (1ul << index)) != 0)
            {
                // Pinned knight can't move
                continue;
            }

            AccumulateBlackKnightMoves(ref board, ref hashEntry, depth, index);
        }
        
        positions = board.BlackBishop;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackBishopMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }
        
        positions = board.BlackRook;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackRookMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }
        
        positions = board.BlackQueen;
        while (positions != 0)
        {
            var index = positions.PopLSB();
            AccumulateBlackQueenMoves(ref board, ref hashEntry, depth, index, (PinMask & (1ul << index)) != 0);
        }
        
        summary.Accumulate(ref hashEntry);
        *ptr = hashEntry;
    }

    private static void AccumulateWhitePawnMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;
        var rankIndex = index.GetRankIndex();
        int toSquare;
        if (rankIndex.IsSeventhRank())
        {
            // Promoting moves
            var validMoves = AttackTables.WhitePawnAttackTable[index] & board.MoveMask & board.Black;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
            }

            while (validMoves != 0)
            {
                if (depth == 1) summary.AddPromotionCapture();
                toSquare = validMoves.PopLSB();

                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_KnightPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_BishopPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_RookPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture_QueenPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }

            validMoves = AttackTables.WhitePawnPushTable[index] & board.MoveMask & ~board.Occupancy;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
            }
            while (validMoves != 0)
            {
                if (depth == 1) summary.AddPromotion();

                toSquare = validMoves.PopLSB();
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_KnightPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_BishopPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_RookPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_QueenPromotion(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }
        }
        else
        {
            var validMoves = AttackTables.WhitePawnAttackTable[index] & board.MoveMask & board.Black;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
            }

            while (validMoves != 0)
            {
                if (depth == 1) summary.AddCapture();

                toSquare = validMoves.PopLSB();
                board.CloneTo(ref newBoard);
                newBoard.WhitePawn_Capture(index, toSquare);
                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }

            if (board.EnPassantFile != 8 && rankIndex.IsWhiteEnPassantRankIndex() &&
                Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
            {
                board.CloneTo(ref newBoard);
                toSquare = Board.WhiteEnpassantOffset + board.EnPassantFile;

                newBoard.WhitePawn_Enpassant(index, toSquare);
                if (!newBoard.IsAttackedByBlack(newBoard.WhiteKingPos))
                {
                    if (depth == 1) summary.AddEnpassant();
                    AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
                }
            }

            validMoves = AttackTables.WhitePawnPushTable[index] & board.MoveMask & ~board.Occupancy;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
            }
            while (validMoves != 0)
            {
                toSquare = validMoves.PopLSB();
                board.CloneTo(ref newBoard);

                if (rankIndex.IsSecondRank() && toSquare.GetRankIndex() == 3)
                {
                    // Double push: Check intermediate square
                    var intermediateSquare = (index + toSquare) / 2; // Midpoint between start and destination
                    if ((board.Occupancy & (1UL << intermediateSquare)) != 0)
                    {
                        continue; // Intermediate square is blocked, skip this move
                    }
                    newBoard.WhitePawn_DoublePush(index, toSquare);
                }
                else
                {
                    // single push
                    newBoard.WhitePawn_Move(index, toSquare);
                }

                AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }
        }
     
    }

    private static void AccumulateWhiteKnightMoves(ref Board board, ref Summary summary, int depth, int index)
    {
        int toSquare;
        Board newBoard = default;
        var potentialMoves = *(AttackTables.KnightAttackTable + index) & board.MoveMask;
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();
            newBoard.WhiteKnight_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();
            newBoard.WhiteKnight_Move(index, toSquare);
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateWhiteBishopMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;
        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & board.MoveMask;
        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index);
        }

        int toSquare;
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();
            newBoard.WhiteBishop_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();
            newBoard.WhiteBishop_Move(index, toSquare);
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateWhiteRookMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;
        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & board.MoveMask;
        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }
        int toSquare;
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();
            newBoard.WhiteRook_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();
            newBoard.WhiteRook_Move(index, toSquare);
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateWhiteQueenMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;

        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & board.MoveMask;

        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.WhiteKingPos, index) | AttackTables.GetRayToEdgeStraight(board.WhiteKingPos, index);
        }
        int toSquare;
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();
            newBoard.WhiteQueen_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();
            newBoard.WhiteQueen_Move(index, toSquare);
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateWhiteKingMoves(ref Board board, ref Summary summary, int depth, int index, bool inCheck)
    {
        Board newBoard = default;
        var attackedSquares = board.WhiteKingDangerSquares();

        var potentialMoves = *(AttackTables.KingAttackTable + index) & ~attackedSquares;
        int toSquare;
        var captureMoves = potentialMoves & board.Black;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();
            newBoard.WhiteKing_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();
            newBoard.WhiteKing_Move(index, toSquare);
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        if (index != 4 || inCheck)
            // Can't castle if king is attacked or not on the starting position
            return;

        if ((board.CastleRights & CastleRights.WhiteKingSide) != 0 &&
            (board.WhiteRook & WhiteKingSideCastleRookPosition) > 0 &&
            (board.Occupancy & WhiteKingSideCastleEmptyPositions) == 0 &&
            (attackedSquares & (1ul << 6)) == 0 &&
            (attackedSquares & (1ul << 5)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_KingSideCastle();
            if (depth == 1) summary.AddCastle();
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, 5);
        }

        // Queen Side Castle
        if ((board.CastleRights & CastleRights.WhiteQueenSide) != 0 &&
            (board.WhiteRook & WhiteQueenSideCastleRookPosition) > 0 &&
            (board.Occupancy & WhiteQueenSideCastleEmptyPositions) == 0 &&
              (attackedSquares & (1ul << 2)) == 0 &&
            (attackedSquares & (1ul << 3)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.WhiteKing_QueenSideCastle();
            if (depth == 1) summary.AddCastle();
            AccumulateBlackMoves(ref newBoard, ref summary, depth - 1, 3);
        }
    }


    private static void AccumulateBlackPawnMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;
        var rankIndex = index.GetRankIndex();
        int toSquare;
        if (rankIndex.IsSecondRank())
        {
            // Promoting moves
            var validMoves = AttackTables.BlackPawnAttackTable[index] & board.MoveMask & board.White;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
            }

            while (validMoves != 0)
            {
                if (depth == 1) summary.AddPromotionCapture();
                toSquare = validMoves.PopLSB();

                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_KnightPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_BishopPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_RookPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture_QueenPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }

            validMoves = AttackTables.BlackPawnPushTable[index] & board.MoveMask & ~board.Occupancy;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
            }
            while (validMoves != 0)
            {
                if (depth == 1) summary.AddPromotion();

                toSquare = validMoves.PopLSB();
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_KnightPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_BishopPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_RookPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_QueenPromotion(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }
        }
        else
        {
            var validMoves = AttackTables.BlackPawnAttackTable[index] & board.MoveMask & board.White;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
            }

            while (validMoves != 0)
            {
                if (depth == 1) summary.AddCapture();

                toSquare = validMoves.PopLSB();
                board.CloneTo(ref newBoard);
                newBoard.BlackPawn_Capture(index, toSquare);
                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }

            if (board.EnPassantFile != 8 && rankIndex.IsBlackEnPassantRankIndex() &&
                Math.Abs(index.GetFileIndex() - board.EnPassantFile) == 1)
            {
                board.CloneTo(ref newBoard);
                toSquare = Board.blackEnpassantOffset + board.EnPassantFile;

                newBoard.BlackPawn_Enpassant(index, toSquare);
                if (!newBoard.IsAttackedByWhite(newBoard.BlackKingPos))
                {
                    if (depth == 1) summary.AddEnpassant();
                    AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
                }
            }

            validMoves = AttackTables.BlackPawnPushTable[index] & board.MoveMask & ~board.Occupancy;
            if (isPinned)
            {
                validMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
            }
            while (validMoves != 0)
            {
                toSquare = validMoves.PopLSB();
                board.CloneTo(ref newBoard);

                if (rankIndex.IsSeventhRank() && toSquare.GetRankIndex() == 4)
                {
                    // Double push: Check intermediate square
                    var intermediateSquare = (index + toSquare) / 2; // Midpoint between start and destination
                    if ((board.Occupancy & (1UL << intermediateSquare)) != 0)
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

                AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
            }
        }

    }

    private static void AccumulateBlackKnightMoves(ref Board board, ref Summary summary, int depth, int index)
    {
        Board newBoard = default;
        int toSquare;

        var potentialMoves = *(AttackTables.KnightAttackTable + index) & board.MoveMask;
        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();

            newBoard.BlackKnight_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();

            newBoard.BlackKnight_Move(index, toSquare);
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateBlackBishopMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextBishopAttacks(board.Occupancy, index) & board.MoveMask;

        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index);
        }

        int toSquare;

        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();

            newBoard.BlackBishop_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();

            newBoard.BlackBishop_Move(index, toSquare);
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateBlackRookMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;

        var potentialMoves = AttackTables.PextRookAttacks(board.Occupancy, index) & board.MoveMask;

        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }
        int toSquare;

        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();

            newBoard.BlackRook_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();

            newBoard.BlackRook_Move(index, toSquare);
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateBlackQueenMoves(ref Board board, ref Summary summary, int depth, int index, bool isPinned)
    {
        Board newBoard = default;

        var potentialMoves = (AttackTables.PextBishopAttacks(board.Occupancy, index) |
                             AttackTables.PextRookAttacks(board.Occupancy, index)) & board.MoveMask;

        if (isPinned)
        {
            potentialMoves &= AttackTables.GetRayToEdgeDiagonal(board.BlackKingPos, index) | AttackTables.GetRayToEdgeStraight(board.BlackKingPos, index);
        }
        int toSquare;

        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();

            newBoard.BlackQueen_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();

            newBoard.BlackQueen_Move(index, toSquare);
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }
    }

    private static void AccumulateBlackKingMoves(ref Board board, ref Summary summary, int depth, int index, bool inCheck)
    {
        var attackedSquares = board.BlackKingDangerSquares();
        Board newBoard = default;

        var potentialMoves = *(AttackTables.KingAttackTable + index) & ~attackedSquares;
        int toSquare;

        var captureMoves = potentialMoves & board.White;
        while (captureMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = captureMoves.PopLSB();
            newBoard.BlackKing_Capture(index, toSquare);
            if (depth == 1) summary.AddCapture();
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        var emptyMoves = potentialMoves & ~board.Occupancy;
        while (emptyMoves != 0)
        {
            board.CloneTo(ref newBoard);
            toSquare = emptyMoves.PopLSB();

            newBoard.BlackKing_Move(index, toSquare);
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, toSquare);
        }

        if (index != 60 || inCheck)
            // Can't castle if king is attacked or not on the starting position
            return;

        // King Side Castle
        if ((board.CastleRights & CastleRights.BlackKingSide) != 0 &&
            (board.BlackRook & BlackKingSideCastleRookPosition) > 0 &&
            (board.Occupancy & BlackKingSideCastleEmptyPositions) == 0 &&
            (attackedSquares & (1ul << 61)) == 0 &&
            (attackedSquares & (1ul << 62)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_KingSideCastle();
            if (depth == 1) summary.AddCastle();
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, 61);
        }

        // Queen Side Castle
        if ((board.CastleRights & CastleRights.BlackQueenSide) != 0 &&
            (board.BlackRook & BlackQueenSideCastleRookPosition) > 0 &&
            (board.Occupancy & BlackQueenSideCastleEmptyPositions) == 0 &&
            (attackedSquares & (1ul << 58)) == 0 &&
            (attackedSquares & (1ul << 59)) == 0)
        {
            board.CloneTo(ref newBoard);
            newBoard.BlackKing_QueenSideCastle();
            if (depth == 1) summary.AddCastle();
            AccumulateWhiteMoves(ref newBoard, ref summary, depth - 1, 59);
        }
    }
}