namespace GrandChessTree.Shared;

[Flags]
public enum CastleRights : byte
{
    None = 0,
    WhiteKingSide = 1,
    WhiteQueenSide = 2,
    BlackKingSide = 4,
    BlackQueenSide = 8
}

public struct Board
{
    public ulong WhitePawn;
    public ulong WhiteKnight;
    public ulong WhiteBishop;
    public ulong WhiteRook;
    public ulong WhiteQueen;
    public ulong WhiteKing;

    public ulong BlackPawn;
    public ulong BlackKnight;
    public ulong BlackBishop;
    public ulong BlackRook;
    public ulong BlackQueen;
    public ulong BlackKing;

    public ulong White;
    public ulong Black;
    public ulong Occupancy;

    public CastleRights CastleRights;
    public byte EnPassantFile;

    public byte WhiteKingPos;
    public byte BlackKingPos;

    public ulong Checkers;
    public byte NumCheckers;
    public ulong AttackedSquares;
    public ulong CaptureMask;
    public ulong PushMask;
    public ulong PinMask;
    public ulong Hash;
    public void UpdateOccupancy()
    {
        White = WhitePawn |
                WhiteKnight |
                WhiteBishop |
                WhiteRook |
                WhiteQueen |
                WhiteKing;

        Black = BlackPawn |
                BlackKnight |
                BlackBishop |
                BlackRook |
                BlackQueen |
                BlackKing;
        Occupancy = White | Black;
    }

    #region moves

    public const int BlackWhiteEnpassantOffset = 5 * 8;

    public unsafe void WhitePawn_Enpassant(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        var captureSquare = 1UL << (fromSquare.GetRankIndex() * 8 + EnPassantFile);

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackPawn + (fromSquare.GetRankIndex() * 8 + EnPassantFile)) ^ Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        WhitePawn ^= moveMask;
        White ^= moveMask;

        BlackPawn ^= captureSquare;
        Black ^= captureSquare;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public unsafe void WhitePawn_Capture_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteKnight |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = (1UL << toSquare);

        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public unsafe void WhitePawn_Capture_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteBishop |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public unsafe void WhitePawn_Capture_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteRook |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public unsafe void WhitePawn_Capture_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteQueen |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhitePawn_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= moveMask;
        White ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }


        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhitePawn_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteKnight |= 1UL << toSquare;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
        *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
        Zobrist.SideToMove ^
        *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhitePawn_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteBishop |= 1UL << toSquare;
        White ^= moveMask;
        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
*(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
Zobrist.SideToMove ^
*(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhitePawn_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteRook |= 1UL << toSquare;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhitePawn_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteQueen |= 1UL << toSquare;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
        *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
        Zobrist.SideToMove ^
        *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhitePawn_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= moveMask;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
            *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
            Zobrist.SideToMove ^
            *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhitePawn_DoublePush(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= moveMask;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhitePawn + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + (byte)(fromSquare % 8));

        Occupancy = Black | White;
        EnPassantFile = (byte)(fromSquare % 8);
    }

    internal unsafe void WhiteKnight_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKnight ^= moveMask;
        White ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKnight + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKnight + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKnight + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKnight + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKnight + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteKnight_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKnight ^= moveMask;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKnight + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteBishop_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteBishop ^= moveMask;
        White ^= moveMask;
        var captureMask = (1UL << toSquare);
        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteBishop + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteBishop + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteBishop + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteBishop + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteBishop + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteBishop_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteBishop ^= moveMask;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteBishop + fromSquare) ^
        *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
        Zobrist.SideToMove ^
        *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteRook_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteRook ^= moveMask;
        White ^= moveMask;

        var prevCastleRights = CastleRights;
        if (fromSquare == 0)
            CastleRights &= ~CastleRights.WhiteQueenSide;
        else if (fromSquare == 7)
            CastleRights &= ~CastleRights.WhiteKingSide;

        var captureMask = (1UL << toSquare);
        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteRook + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                    *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteRook + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                    *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteRook + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                    *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteRook + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                    *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteRook + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                    *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteRook_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteRook ^= moveMask;
        White ^= moveMask;

        var prevCastleRights = CastleRights;
        if (fromSquare == 0)
            CastleRights &= ~CastleRights.WhiteQueenSide;
        else if (fromSquare == 7)
            CastleRights &= ~CastleRights.WhiteKingSide;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteRook + fromSquare) ^
        *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
        Zobrist.SideToMove ^
        *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                    *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteQueen_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteQueen ^= moveMask;
        White ^= moveMask;

        var captureMask = (1UL << toSquare);

        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteQueen + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteQueen + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteQueen + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteQueen + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteQueen + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteQueen_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteQueen ^= moveMask;
        White ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteQueen + fromSquare) ^
        *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
        Zobrist.SideToMove ^
        *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void WhiteKing_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKing ^= moveMask;
        White ^= moveMask;
        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);

        var captureMask = (1UL << toSquare);

        if ((BlackPawn & captureMask) != 0)
        {
            BlackPawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKing + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((BlackKnight & captureMask) != 0)
        {
            BlackKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteKing + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        }
        else if ((BlackBishop & captureMask) != 0)
        {
            BlackBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + fromSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.WhiteKing + toSquare) ^
                   *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((BlackRook & captureMask) != 0)
        {
            BlackRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKing + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((BlackQueen & captureMask) != 0)
        {
            BlackQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhiteKing + toSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }

        Black &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
        WhiteKingPos = (byte)toSquare;
    }

    internal unsafe void WhiteKing_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKing ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;
        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + fromSquare) ^
        *(Zobrist.PiecesArray + Zobrist.WhiteKing + toSquare) ^
        Zobrist.SideToMove ^
        *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));


        EnPassantFile = 8;
        WhiteKingPos = (byte)toSquare;
    }

    internal unsafe void WhiteKing_KingSideCastle()
    {
        WhiteKing ^= (1UL << 4) | (1UL << 6);
        WhiteRook ^= (1UL << 7) | (1UL << 5);
        White ^= (1UL << 4) | (1UL << 7);
        White |= (1UL << 6) | (1UL << 5);

        Occupancy = Black | White;
        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + 4) ^
                 *(Zobrist.PiecesArray + Zobrist.WhiteKing + 6) ^
                 *(Zobrist.PiecesArray + Zobrist.WhiteRook + 7) ^
                 *(Zobrist.PiecesArray + Zobrist.WhiteRook + 5) ^
                 Zobrist.SideToMove ^
                 *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                 *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        EnPassantFile = 8;
        WhiteKingPos = 6;
    }

    internal unsafe void WhiteKing_QueenSideCastle()
    {
        WhiteKing ^= (1UL << 4) | (1UL << 2);
        WhiteRook ^= (1UL << 0) | (1UL << 3);
        White ^= (1UL << 4) | (1UL << 0);
        White |= (1UL << 2) | (1UL << 3);

        Occupancy = Black | White;

        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);

        Hash ^= *(Zobrist.PiecesArray + Zobrist.WhiteKing + 4) ^
                 *(Zobrist.PiecesArray + Zobrist.WhiteKing + 2) ^
                 *(Zobrist.PiecesArray + Zobrist.WhiteRook + 0) ^
                 *(Zobrist.PiecesArray + Zobrist.WhiteRook + 3) ^
                 Zobrist.SideToMove ^
                 *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                 *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        EnPassantFile = 8;
        WhiteKingPos = 2;
    }

    internal unsafe void BlackKnight_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKnight ^= moveMask;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKnight + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKnight + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKnight + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKnight + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKnight + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackKnight_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKnight ^= moveMask;
        Black ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKnight + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackBishop_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackBishop ^= moveMask;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackBishop + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackBishop + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackBishop + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackBishop + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackBishop + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackBishop_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackBishop ^= moveMask;
        Black ^= moveMask;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackBishop + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackRook_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackRook ^= moveMask;
        Black ^= moveMask;

        var prevCastleRights = CastleRights;
        if (fromSquare == 56)
            CastleRights &= ~CastleRights.BlackQueenSide;
        else if (fromSquare == 63)
            CastleRights &= ~CastleRights.BlackKingSide;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackRook + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackRook + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackRook + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackRook + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackRook + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackRook_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackRook ^= moveMask;
        Black ^= moveMask;
        var prevCastleRights = CastleRights;

        if (fromSquare == 56)
            CastleRights &= ~CastleRights.BlackQueenSide;
        else if (fromSquare == 63)
            CastleRights &= ~CastleRights.BlackKingSide;

        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackRook + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        Occupancy = Black | White;


        EnPassantFile = 8;
    }

    internal unsafe void BlackQueen_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackQueen ^= moveMask;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackQueen + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackQueen + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackQueen + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackQueen + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackQueen + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackQueen_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackQueen ^= moveMask;
        Black ^= moveMask;


        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackQueen + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackKing_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKing ^= moveMask;
        Black ^= moveMask;

        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKing + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKing + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKing + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKing + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKing + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
        BlackKingPos = (byte)toSquare;
    }

    internal unsafe void BlackKing_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKing ^= moveMask;
        Black ^= moveMask;
        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);

        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackKing + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
        *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        Occupancy = Black | White;
        EnPassantFile = 8;
        BlackKingPos = (byte)toSquare;
    }

    internal unsafe void BlackKing_KingSideCastle()
    {
        BlackKing ^= (1UL << 60) | (1UL << 62);
        BlackRook ^= (1UL << 63) | (1UL << 61);
        Black ^= (1UL << 60) | (1UL << 63);
        Black |= (1UL << 62) | (1UL << 61);

        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);

        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + 60) ^
                 *(Zobrist.PiecesArray + Zobrist.BlackKing + 62) ^
                 *(Zobrist.PiecesArray + Zobrist.BlackRook + 63) ^
                 *(Zobrist.PiecesArray + Zobrist.BlackRook + 61) ^
                 Zobrist.SideToMove ^
                 *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                 *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        Occupancy = Black | White;
        EnPassantFile = 8;
        BlackKingPos = 62;
    }

    internal unsafe void BlackKing_QueenSideCastle()
    {
        BlackKing ^= (1UL << 60) | (1UL << 58);
        BlackRook ^= (1UL << 56) | (1UL << 59);
        Black ^= (1UL << 60) | (1UL << 56);
        Black |= (1UL << 58) | (1UL << 59);

        var prevCastleRights = CastleRights;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);

        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackKing + 60) ^
                 *(Zobrist.PiecesArray + Zobrist.BlackKing + 58) ^
                 *(Zobrist.PiecesArray + Zobrist.BlackRook + 56) ^
                 *(Zobrist.PiecesArray + Zobrist.BlackRook + 59) ^
                 Zobrist.SideToMove ^
                 *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8) ^
                 *(Zobrist.DeltaCastleRights + (int)(prevCastleRights ^ CastleRights));

        Occupancy = Black | White;
        EnPassantFile = 8;
        BlackKingPos = 58;
    }

    public const int blackEnpassantOffset = 2 * 8;

    internal unsafe void BlackPawn_Enpassant(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        var captureSquare = ~(1UL << (byte)(fromSquare.GetRankIndex() * 8 + EnPassantFile));

        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                *(Zobrist.PiecesArray + Zobrist.WhitePawn + (fromSquare.GetRankIndex() * 8 + EnPassantFile)) ^ Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        BlackPawn ^= moveMask;
        Black ^= moveMask;

        WhitePawn &= captureSquare;
        White &= captureSquare;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_Capture_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackQueen |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }


        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_Capture_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackRook |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_Capture_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackBishop |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_Capture_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackKnight |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= moveMask;
        Black ^= moveMask;

        var captureMask = (1UL << toSquare);
        if ((WhitePawn & captureMask) != 0)
        {
            WhitePawn &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhitePawn + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        }
        else if ((WhiteKnight & captureMask) != 0)
        {
            WhiteKnight &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteKnight + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteBishop & captureMask) != 0)
        {
            WhiteBishop &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteBishop + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteRook & captureMask) != 0)
        {
            WhiteRook &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteRook + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }
        else if ((WhiteQueen & captureMask) != 0)
        {
            WhiteQueen &= ~captureMask;
            Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                    *(Zobrist.PiecesArray + Zobrist.WhiteQueen + toSquare) ^
                    Zobrist.SideToMove ^
                    *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        }

        White &= ~captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackKnight |= 1UL << toSquare;
        Black ^= moveMask;
        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
*(Zobrist.PiecesArray + Zobrist.BlackKnight + toSquare) ^
Zobrist.SideToMove ^
*(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackBishop |= 1UL << toSquare;
        Black ^= moveMask;
        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
*(Zobrist.PiecesArray + Zobrist.BlackBishop + toSquare) ^
Zobrist.SideToMove ^
*(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackRook |= 1UL << toSquare;
        Black ^= moveMask;
        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
*(Zobrist.PiecesArray + Zobrist.BlackRook + toSquare) ^
Zobrist.SideToMove ^
*(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackQueen |= 1UL << toSquare;
        Black ^= moveMask;
        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
*(Zobrist.PiecesArray + Zobrist.BlackQueen + toSquare) ^
Zobrist.SideToMove ^
*(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= moveMask;
        Black ^= moveMask;
        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                Zobrist.SideToMove ^
                *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + 8);
        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal unsafe void BlackPawn_DoublePush(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= moveMask;
        Black ^= moveMask;
        Hash ^= *(Zobrist.PiecesArray + Zobrist.BlackPawn + fromSquare) ^
                *(Zobrist.PiecesArray + Zobrist.BlackPawn + toSquare) ^
                Zobrist.SideToMove ^
                        *(Zobrist.DeltaEnpassant + EnPassantFile * 9 + (byte)(fromSquare % 8));

        Occupancy = Black | White;
        EnPassantFile = (byte)(fromSquare % 8);
    }

    #endregion
}