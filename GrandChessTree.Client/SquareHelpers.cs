using System.Runtime.CompilerServices;

namespace GrandChessTree.Client;

public static class SquareHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanEnPassant(this int enpassantFile)
    {
        return enpassantFile < 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFileIndex(this int square)
    {
        // File is the last 3 bits of the square index
        return square & 7; // Equivalent to square % 8
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetRankIndex(this int square)
    {
        // Rank is obtained by shifting right by 3 bits
        return square >> 3; // Equivalent to square / 8
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetFileIndex(this uint square)
    {
        // File is the last 3 bits of the square index
        return square & 7; // Equivalent to square % 8
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMirroredSide(this int square)
    {
        return (square & 7) >= 4;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetRankIndex(this uint square)
    {
        // Rank is obtained by shifting right by 3 bits
        return square >> 3; // Equivalent to square / 8
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetRankIndex(this ushort square)
    {
        // Rank is obtained by shifting right by 3 bits
        return (ushort)(square >> 3); // Equivalent to square / 8
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSecondRank(this int rankIndex)
    {
        return rankIndex == 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSeventhRank(this int rankIndex)
    {
        return rankIndex == 6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhiteEnPassantRankIndex(this int rankIndex)
    {
        return rankIndex == 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBlackEnPassantRankIndex(this int rankIndex)
    {
        return rankIndex == 3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftUp(this int board)
    {
        return board + 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftDown(this int board)
    {
        return board - 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftLeft(this int board)
    {
        return board - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftRight(this int board)
    {
        return board + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftUpRight(this int board)
    {
        return board + 9;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftUpLeft(this int board)
    {
        return board + 7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftDownRight(this int board)
    {
        return board - 7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ShiftDownLeft(this int board)
    {
        return board - 9;
    }
}