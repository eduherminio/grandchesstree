using System.Runtime.CompilerServices;

namespace GrandChessTree.Shared;

public static class BoardExtensions
{
    public static Board CreateInitialBoard()
    {
        Board board = default;

        board.WhitePawn = 0xFF00;
        board.WhiteKnight = 0x42;
        board.WhiteBishop = 0x24;
        board.WhiteRook = 0x81;
        board.WhiteQueen = 0x8;
        board.WhiteKing = 0x10;

        board.BlackPawn = 0xFF000000000000;
        board.BlackKnight = 0x4200000000000000;
        board.BlackBishop = 0x2400000000000000;
        board.BlackRook = 0x8100000000000000;
        board.BlackQueen = 0x800000000000000;
        board.BlackKing = 0x1000000000000000;

        board.UpdateOccupancy();
        return board;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CloneTo(this ref Board board, ref Board copy)
    {
        fixed (Board* sourcePtr = &board)
        fixed (Board* destPtr = &copy)
        {
            // Copy the memory block from source to destination
            Buffer.MemoryCopy(sourcePtr, destPtr, sizeof(Board), sizeof(Board));
        }
    }
}