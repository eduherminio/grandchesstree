namespace GrandChessTree.Shared.Helpers;

public static class FenParser
{
    public static (Board, bool whiteToMove) Parse(string fen)
    {
        Board board = default;
        var parts = fen.Split(' ');

        var rows = parts[0].Split('/');
        var turn = parts[1];
        var castleRights = parts[2];
        var enPassantTarget = parts[3];

        var index = 0;
        for (var i = rows.Length - 1; i >= 0; i--)
        {
            var row = rows[i];
            foreach (var c in row)
                if (char.IsDigit(c))
                {
                    index += (int)char.GetNumericValue(c);
                }
                else
                {
                    switch (c)
                    {
                        case 'P':
                            board.WhitePawn |= 1UL << index;
                            break;
                        case 'N':
                            board.WhiteKnight |= 1UL << index;
                            break;
                        case 'B':
                            board.WhiteBishop |= 1UL << index;
                            break;
                        case 'R':
                            board.WhiteRook |= 1UL << index;
                            break;
                        case 'Q':
                            board.WhiteQueen |= 1UL << index;
                            break;
                        case 'K':
                            board.WhiteKingPos = (byte)index;
                            break;
                        case 'p':
                            board.BlackPawn |= 1UL << index;
                            break;
                        case 'n':
                            board.BlackKnight |= 1UL << index;
                            break;
                        case 'b':
                            board.BlackBishop |= 1UL << index;
                            break;
                        case 'r':
                            board.BlackRook |= 1UL << index;
                            break;
                        case 'q':
                            board.BlackQueen |= 1UL << index;
                            break;
                        case 'k':
                            board.BlackKingPos = (byte)index;
                            break;
                        default:
                            throw new Exception($"invalid piece '{c}'");
                    }

                    index++;
                }
        }

        board.UpdateOccupancy();
        board.CastleRights = ParseCastleRights(castleRights);

        if (enPassantTarget == "-")
            board.EnPassantFile = 8;
        else
            board.EnPassantFile = (byte)"abcdefgh".IndexOf(enPassantTarget[0]);

        return (board, turn == "w");
    }


    private static CastleRights ParseCastleRights(string input)
    {
        var castleRights = CastleRights.None;

        foreach (var right in input)
            if (right == 'K')
                castleRights |= CastleRights.WhiteKingSide;
            else if (right == 'Q')
                castleRights |= CastleRights.WhiteQueenSide;
            else if (right == 'k')
                castleRights |= CastleRights.BlackKingSide;
            else if (right == 'q') castleRights |= CastleRights.BlackQueenSide;

        return castleRights;
    }
}