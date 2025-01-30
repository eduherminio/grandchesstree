using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;

namespace GrandChessTree.Shared;
public static unsafe class Perft
{
    #region HashTable
    public static readonly Summary* HashTable;
    public static readonly uint HashTableMask;   
   

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

        var bytes = ((nuint)sizeof(Summary) * items);
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
            var checkers = board.BlackCheckers();
            var numCheckers = (byte)ulong.PopCount(checkers);
            board.MoveMask = numCheckers == 0 ? 0xFFFFFFFFFFFFFFFF: checkers | *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(checkers));
            var pinMask = board.WhiteKingPinnedRay();

            var positions = board.WhitePawn;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhitePawnMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.WhiteKnight;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                if ((pinMask & (1ul << index)) != 0)
                {
                    // Pinned knight can't move
                    continue;
                }
                board.AccumulateWhiteKnightMoves(ref summary, depth, index);
            }

            positions = board.WhiteBishop;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhiteBishopMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.WhiteRook;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhiteRookMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.WhiteQueen;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhiteQueenMoves( ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            board.AccumulateWhiteKingMoves(ref summary, depth, numCheckers > 0);
            return summary;
        }
        else
        {
            var checkers = board.WhiteCheckers();
            var numCheckers = (byte)ulong.PopCount(checkers);
            board.MoveMask = 0xFFFFFFFFFFFFFFFF;
            if (numCheckers == 1)
            {
                board.MoveMask = checkers | *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(checkers));
            }
            var pinMask = board.BlackKingPinnedRay();

            var positions = board.BlackPawn;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackPawnMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.BlackKnight;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                if ((pinMask & (1ul << index)) != 0)
                {
                    // Pinned knight can't move
                    continue;
                }

                board.AccumulateBlackKnightMoves(ref summary, depth, index);
            }

            positions = board.BlackBishop;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackBishopMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.BlackRook;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackRookMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.BlackQueen;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackQueenMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }


            board.AccumulateBlackKingMoves(ref summary, depth, board.BlackKingPos, numCheckers > 0);
            return summary;
        }
    }

  
}