using System.Runtime.CompilerServices;

namespace GrandChessTree.Shared;

public static class BitboardHelpers
{
    private const ulong NotAFile = 0xFEFEFEFEFEFEFEFE; // All squares except column 'A'
    private const ulong NotHFile = 0x7F7F7F7F7F7F7F7F; // All squares except column 'H'

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong RankFileToBitboard(int rank, int file)
    {
        return 1UL << (rank * 8 + file);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SquareToBitboard(this int square)
    {
        return 1UL << square;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftUp(this ulong board)
    {
        return board << 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftDown(this ulong board)
    {
        return board >> 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftLeft(this ulong board)
    {
        return (board >> 1) & NotHFile;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftRight(this ulong board)
    {
        return (board << 1) & NotAFile;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftUpRight(this ulong board)
    {
        // Combined shift up (<< 8) and right (<< 1) with a mask to prevent overflow on the left side.
        return (board << 9) & NotAFile;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftUpLeft(this ulong board)
    {
        // Combined shift up (<< 8) and left (>> 1) with a mask to prevent overflow on the right side.
        return (board << 7) & NotHFile;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftDownRight(this ulong board)
    {
        // Combined shift down (>> 8) and right (<< 1) with a mask to prevent overflow on the left side.
        return (board >> 7) & NotAFile;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftDownLeft(this ulong board)
    {
        // Combined shift down (>> 8) and left (>> 1) with a mask to prevent overflow on the right side.
        return (board >> 9) & NotHFile;
    }
}