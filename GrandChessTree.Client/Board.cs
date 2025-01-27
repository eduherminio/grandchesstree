namespace GrandChessTree.Client;

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

    public int WhiteKingPos;
    public int BlackKingPos;

    public ulong Checkers;
    public ulong NumCheckers;
    public ulong AttackedSquares;
    public ulong CaptureMask;
    public ulong PushMask;
    public ulong PinMask;

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

    private const int BlackWhiteEnpassantOffset = 5 * 8;

    public void WhitePawn_Enpassant(int fromSquare)
    {
        var toSquare = BlackWhiteEnpassantOffset + EnPassantFile;
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);

        WhitePawn ^= moveMask;
        White ^= moveMask;

        var captureSquare = 1UL << (fromSquare.GetRankIndex() * 8 + EnPassantFile);
        BlackPawn ^= captureSquare;
        Black ^= captureSquare;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public void WhitePawn_Capture_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteKnight |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public void WhitePawn_Capture_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteBishop |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public void WhitePawn_Capture_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteRook |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    public void WhitePawn_Capture_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteQueen |= 1UL << toSquare;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhitePawn_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= moveMask;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhitePawn_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteKnight |= 1UL << toSquare;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhitePawn_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteBishop |= 1UL << toSquare;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhitePawn_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteRook |= 1UL << toSquare;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhitePawn_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= 1UL << fromSquare;
        WhiteQueen |= 1UL << toSquare;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhitePawn_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhitePawn_DoublePush(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhitePawn ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = (byte)(fromSquare % 8);
    }

    internal void WhiteKnight_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKnight ^= moveMask;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhiteKnight_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKnight ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhiteBishop_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteBishop ^= moveMask;
        White ^= moveMask;
        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhiteBishop_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteBishop ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhiteRook_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteRook ^= moveMask;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;


        if (fromSquare == 0)
            CastleRights &= ~CastleRights.WhiteQueenSide;
        else if (fromSquare == 7) CastleRights &= ~CastleRights.WhiteKingSide;
        EnPassantFile = 8;
    }

    internal void WhiteRook_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteRook ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;

        if (fromSquare == 0)
            CastleRights &= ~CastleRights.WhiteQueenSide;
        else if (fromSquare == 7) CastleRights &= ~CastleRights.WhiteKingSide;
        EnPassantFile = 8;
    }

    internal void WhiteQueen_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteQueen ^= moveMask;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhiteQueen_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteQueen ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void WhiteKing_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKing ^= moveMask;
        White ^= moveMask;

        var captureMask = ~(1UL << toSquare);

        BlackPawn &= captureMask;
        BlackKnight &= captureMask;
        BlackBishop &= captureMask;
        BlackRook &= captureMask;
        BlackQueen &= captureMask;
        Black &= captureMask;

        Occupancy = Black | White;
        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);
        EnPassantFile = 8;
        WhiteKingPos = toSquare;
    }

    internal void WhiteKing_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        WhiteKing ^= moveMask;
        White ^= moveMask;

        Occupancy = Black | White;

        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);
        EnPassantFile = 8;
        WhiteKingPos = toSquare;
    }

    internal void WhiteKing_KingSideCastle()
    {
        WhiteKing ^= (1UL << 4) | (1UL << 6);
        WhiteRook ^= (1UL << 7) | (1UL << 5);
        White ^= (1UL << 4) | (1UL << 7);
        White |= (1UL << 6) | (1UL << 5);

        Occupancy = Black | White;

        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);
        EnPassantFile = 8;
        WhiteKingPos = 6;
    }

    internal void WhiteKing_QueenSideCastle()
    {
        WhiteKing ^= (1UL << 4) | (1UL << 2);
        WhiteRook ^= (1UL << 0) | (1UL << 3);
        White ^= (1UL << 4) | (1UL << 0);
        White |= (1UL << 2) | (1UL << 3);

        Occupancy = Black | White;

        CastleRights &= ~(CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide);
        EnPassantFile = 8;
        WhiteKingPos = 2;
    }

    internal void BlackKnight_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKnight ^= moveMask;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackKnight_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKnight ^= moveMask;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackBishop_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackBishop ^= moveMask;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackBishop_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackBishop ^= moveMask;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackRook_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackRook ^= moveMask;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;

        if (fromSquare == 56)
            CastleRights &= ~CastleRights.BlackQueenSide;
        else if (fromSquare == 63) CastleRights &= ~CastleRights.BlackKingSide;
        EnPassantFile = 8;
    }

    internal void BlackRook_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackRook ^= moveMask;
        Black ^= moveMask;

        Occupancy = Black | White;

        if (fromSquare == 56)
            CastleRights &= ~CastleRights.BlackQueenSide;
        else if (fromSquare == 63) CastleRights &= ~CastleRights.BlackKingSide;
        EnPassantFile = 8;
    }

    internal void BlackQueen_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackQueen ^= moveMask;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackQueen_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackQueen ^= moveMask;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackKing_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKing ^= moveMask;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);
        EnPassantFile = 8;
        BlackKingPos = toSquare;
    }

    internal void BlackKing_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackKing ^= moveMask;
        Black ^= moveMask;

        Occupancy = Black | White;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);
        EnPassantFile = 8;
        BlackKingPos = toSquare;
    }

    internal void BlackKing_KingSideCastle()
    {
        BlackKing ^= (1UL << 60) | (1UL << 62);
        BlackRook ^= (1UL << 63) | (1UL << 61);
        Black ^= (1UL << 60) | (1UL << 63);
        Black |= (1UL << 62) | (1UL << 61);

        Occupancy = Black | White;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);
        EnPassantFile = 8;
        BlackKingPos = 62;
    }

    internal void BlackKing_QueenSideCastle()
    {
        BlackKing ^= (1UL << 60) | (1UL << 58);
        BlackRook ^= (1UL << 56) | (1UL << 59);
        Black ^= (1UL << 60) | (1UL << 56);
        Black |= (1UL << 58) | (1UL << 59);

        Occupancy = Black | White;
        CastleRights &= ~(CastleRights.BlackKingSide | CastleRights.BlackQueenSide);
        EnPassantFile = 8;
        BlackKingPos = 58;
    }

    private const int blackEnpassantOffset = 2 * 8;

    internal void BlackPawn_Enpassant(int fromSquare)
    {
        var toSquare = blackEnpassantOffset + EnPassantFile;
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);

        BlackPawn ^= moveMask;
        Black ^= moveMask;

        var captureSquare = ~(1UL << (byte)(fromSquare.GetRankIndex() * 8 + EnPassantFile));
        WhitePawn &= captureSquare;
        White &= captureSquare;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_Capture_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackQueen |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_Capture_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackRook |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_Capture_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackBishop |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_Capture_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackKnight |= 1UL << toSquare;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_Capture(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= moveMask;
        Black ^= moveMask;

        var captureMask = ~(1UL << toSquare);
        WhitePawn &= captureMask;
        WhiteKnight &= captureMask;
        WhiteBishop &= captureMask;
        WhiteRook &= captureMask;
        WhiteQueen &= captureMask;
        White &= captureMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_KnightPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackKnight |= 1UL << toSquare;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_BishopPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackBishop |= 1UL << toSquare;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_RookPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackRook |= 1UL << toSquare;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_QueenPromotion(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= 1UL << fromSquare;
        BlackQueen |= 1UL << toSquare;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_Move(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= moveMask;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = 8;
    }

    internal void BlackPawn_DoublePush(int fromSquare, int toSquare)
    {
        var moveMask = (1UL << fromSquare) | (1UL << toSquare);
        BlackPawn ^= moveMask;
        Black ^= moveMask;

        Occupancy = Black | White;
        EnPassantFile = (byte)(fromSquare % 8);
    }

    #endregion
}