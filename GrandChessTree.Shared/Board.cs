using System.Runtime.InteropServices;

namespace GrandChessTree.Shared;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public partial struct Board
{
    // White Pieces
    [FieldOffset(0)] public ulong WhitePawn;      // Offset: 0
    [FieldOffset(8)] public ulong WhiteKnight;    // Offset: 8
    [FieldOffset(16)] public ulong WhiteBishop;    // Offset: 16
    [FieldOffset(24)] public ulong WhiteRook;      // Offset: 24
    [FieldOffset(32)] public ulong WhiteQueen;     // Offset: 32

    // Black Pieces
    [FieldOffset(40)] public ulong BlackPawn;      // Offset: 40
    [FieldOffset(48)] public ulong BlackKnight;    // Offset: 48
    [FieldOffset(56)] public ulong BlackBishop;    // Offset: 56
    [FieldOffset(64)] public ulong BlackRook;      // Offset: 64
    [FieldOffset(72)] public ulong BlackQueen;     // Offset: 72

    // Occupancy Information
    [FieldOffset(80)] public ulong White;          // Offset: 80
    [FieldOffset(88)] public ulong Black;          // Offset: 88
    [FieldOffset(96)] public ulong Occupancy;      // Offset: 96

    // Game State
    [FieldOffset(104)] public CastleRights CastleRights;   // Offset: 104 (1 byte)
    [FieldOffset(105)] public byte EnPassantFile;  // Offset: 105 (1 byte)
    [FieldOffset(106)] public byte WhiteKingPos;   // Offset: 106 (1 byte)
    [FieldOffset(107)] public byte BlackKingPos;   // Offset: 107 (1 byte)

    // Move Mask & Hash
    [FieldOffset(112)] public ulong MoveMask;      // Offset: 112
    [FieldOffset(120)] public ulong Hash;          // Offset: 120

    public void UpdateOccupancy()
    {
        White = WhitePawn |
                WhiteKnight |
                WhiteBishop |
                WhiteRook |
                WhiteQueen |
                (1ul << WhiteKingPos);

        Black = BlackPawn |
                BlackKnight |
                BlackBishop |
                BlackRook |
                BlackQueen |
                (1ul << BlackKingPos);
        Occupancy = White | Black;
    }
}


[Flags]
public enum CastleRights : byte
{
    None = 0,
    WhiteKingSide = 1,
    WhiteQueenSide = 2,
    BlackKingSide = 4,
    BlackQueenSide = 8
}