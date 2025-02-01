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
    
    public static void PerftRoot(ref Board board, ref Summary summary, int depth, bool whiteToMove)
    {
        if (depth == 0)
        {
            // perft(0) = 1
            summary.Nodes++;
            return;
        }

        if (whiteToMove)
        {
            var checkers = board.BlackCheckers();
            var numCheckers = (byte)ulong.PopCount(checkers);

            board.AccumulateWhiteKingMoves(ref summary, depth, numCheckers > 0);

            if (numCheckers > 1)
            {
                // Only a king move can evade double check
                return;
            }

            board.MoveMask = numCheckers == 0 ? 0xFFFFFFFFFFFFFFFF: checkers | *(AttackTables.LineBitBoardsInclusive + board.WhiteKingPos * 64 + Bmi1.X64.TrailingZeroCount(checkers));
            var pinMask = board.WhiteKingPinnedRay();

            var positions = board.White & board.Pawn;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhitePawnMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.White & board.Knight;
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

            positions = board.White & board.Bishop;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhiteBishopMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.White & board.Rook;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhiteRookMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.White & board.Queen;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateWhiteQueenMoves( ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            return;
        }
        else
        {
            var checkers = board.WhiteCheckers();
            var numCheckers = (byte)ulong.PopCount(checkers);

            board.AccumulateBlackKingMoves(ref summary, depth, numCheckers > 0);

            if (numCheckers > 1)
            {
                // Only a king move can evade double check
                return;

            }


            board.MoveMask = 0xFFFFFFFFFFFFFFFF;
            if (numCheckers == 1)
            {
                board.MoveMask = checkers | *(AttackTables.LineBitBoardsInclusive + board.BlackKingPos * 64 + Bmi1.X64.TrailingZeroCount(checkers));
            }
            var pinMask = board.BlackKingPinnedRay();

            var positions = board.Black & board.Pawn;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackPawnMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.Black & board.Knight;
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

            positions = board.Black & board.Bishop;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackBishopMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.Black & board.Rook;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackRookMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            positions = board.Black & board.Queen;
            while (positions != 0)
            {
                var index = positions.PopLSB();
                board.AccumulateBlackQueenMoves(ref summary, depth, index, (pinMask & (1ul << index)) != 0);
            }

            return;
        }
    }

  
}