using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

public static class BitboardHelpers
{
    public const ulong NotAFile = 0xFEFEFEFEFEFEFEFE; // All squares except column 'A'
    public const ulong NotHFile = 0x7F7F7F7F7F7F7F7F; // All squares except column 'H'


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte PopLSB(this ref ulong b)
    {
        var i = (byte)Bmi1.X64.TrailingZeroCount(b);
        b &= b - 1;

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong PeekLSB(this ulong bitBoard)
    {
        return Bmi1.X64.TrailingZeroCount(bitBoard);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte PopCount(ulong bitBoard)
    {
        return (byte)Popcnt.X64.PopCount(bitBoard);
    }

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