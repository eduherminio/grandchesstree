using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace GrandChessTree.Shared;

public static class Helpers
{
    private const byte BlackPawn = 1;
    private const byte BlackKnight = 3;
    private const byte BlackRook = 7;
    private const byte BlackBishop = 5;
    private const byte BlackQueen = 9;
    private const byte BlackKing = 11;

    private const byte WhitePawn = 2;
    private const byte WhiteKnight = 4;
    private const byte WhiteBishop = 6;
    private const byte WhiteRook = 8;
    private const byte WhiteQueen = 10;
    private const byte WhiteKing = 12;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopLSB(this ref ulong b)
    {
        var i = (int)Bmi1.X64.TrailingZeroCount(b);
        b &= b - 1;

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetPiece(this ref Board board, int square)
    {
        if ((board.Occupancy & (1UL << square)) == 0) return 0;

        return (byte)Bmi1.X64.TrailingZeroCount(
            (((board.BlackPawn >> square) & 1UL) << 1) |
            (((board.WhitePawn >> square) & 1UL) << 2) |
            (((board.BlackKnight >> square) & 1UL) << 3) |
            (((board.WhiteKnight >> square) & 1UL) << 4) |
            (((board.BlackBishop >> square) & 1UL) << 5) |
            (((board.WhiteBishop >> square) & 1UL) << 6) |
            (((board.BlackRook >> square) & 1UL) << 7) |
            (((board.WhiteRook >> square) & 1UL) << 8) |
            (((board.BlackQueen >> square) & 1UL) << 9) |
            (((board.WhiteQueen >> square) & 1UL) << 10) |
            (((board.BlackKing >> square) & 1UL) << 11) |
            (((board.WhiteKing >> square) & 1UL) << 12));
    }

    public static string ConvertPosition(this int position)
    {
        var rank = position.GetRankIndex();
        var file = position.GetFileIndex();
        return $"{(char)('a' + file)}{(char)('1' + rank)}";
    }  
    
    public static string ToFen(this Board board, bool whiteToMove)
    {
        var fen = new StringBuilder();

        for (var row = 7; row >= 0; row--)
        {
            var emptyCount = 0;

            for (var col = 0; col < 8; col++)
            {
                var piece = board.GetPiece(row * 8 + col);
                if (piece == 0)
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        fen.Append(emptyCount);
                        emptyCount = 0;
                    }

                    fen.Append(piece.PieceToChar());
                }
            }

            if (emptyCount > 0) fen.Append(emptyCount);

            if (row > 0) fen.Append('/');
        }

        fen.Append(' ');
        fen.Append(whiteToMove ? "w" : "b");
        fen.Append(' ');

        if (board.CastleRights == CastleRights.None)
        {
            fen.Append('-');
        }
        else
        {
            if (board.CastleRights.HasFlag(CastleRights.WhiteKingSide)) fen.Append("K");

            if (board.CastleRights.HasFlag(CastleRights.WhiteQueenSide)) fen.Append('Q');

            if (board.CastleRights.HasFlag(CastleRights.BlackKingSide)) fen.Append('k');

            if (board.CastleRights.HasFlag(CastleRights.BlackQueenSide)) fen.Append('q');
        }
        fen.Append(' ');

        if (board.EnPassantFile >= 8)
            fen.Append("-");
        else
        {
            var enpassantTargetSquare = whiteToMove ? 5 * 8 + board.EnPassantFile : 2 * 8 + board.EnPassantFile;
            fen.Append((enpassantTargetSquare).ConvertPosition());
            fen.Append(' ');
        }

        //fen.Append(board.HalfMoveClock);
        fen.Append(' ');
        //fen.Append(board.TurnCount);

        return fen.ToString();
    }

    private static char PieceToChar(this byte piece)
    {
        return piece switch
        {
            BlackPawn => 'p',
            BlackRook => 'r',
            BlackKnight => 'n',
            BlackBishop => 'b',
            BlackQueen => 'q',
            BlackKing => 'k',
            WhitePawn => 'P',
            WhiteRook => 'R',
            WhiteKnight => 'N',
            WhiteBishop => 'B',
            WhiteQueen => 'Q',
            WhiteKing => 'K',
            _ => '1'
        };
    }

    public static string FormatBigNumber(this float number)
    {
        if (number >= 1000000000000) return (number / 1000000000000D).ToString("0.#") + "t";  // Trillion

        if (number >= 1000000000) return (number / 1000000000D).ToString("0.#") + "b";  // Billion

        if (number >= 1000000) return (number / 1000000D).ToString("0.#") + "m";  // Million

        if (number >= 1000) return (number / 1000D).ToString("0.#") + "k";  // Thousand

        return number.ToString();
    }

    public static string FormatBigNumber(this ulong number)
    {
        if (number >= 1000000000000) return (number / 1000000000000D).ToString("0.#") + "t";  // Trillion

        if (number >= 1000000000) return (number / 1000000000D).ToString("0.#") + "b";  // Billion

        if (number >= 1000000) return (number / 1000000D).ToString("0.#") + "m";  // Million

        if (number >= 1000) return (number / 1000D).ToString("0.#") + "k";  // Thousand

        return number.ToString();
    }

}