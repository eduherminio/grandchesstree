using GrandChessTree.Shared;
using GrandChessTree.Shared.Helpers;

namespace GrandChessTree.Client.Tests
{
    public class FenTests
    {
        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide | CastleRights.BlackKingSide | CastleRights.BlackQueenSide)]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1", CastleRights.None)]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w K - 0 1", CastleRights.WhiteKingSide)]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQ - 0 1", CastleRights.WhiteKingSide | CastleRights.WhiteQueenSide)]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w kq - 0 1", CastleRights.BlackKingSide | CastleRights.BlackQueenSide)]

        public void Parses_CastleRights(string fen, CastleRights expected)
        {
            // Given
            // When
            var (board, _) = FenParser.Parse(fen);

            // Then
            Assert.Equal(expected, board.CastleRights);
        }

        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", 4)]
        [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq a2 0 1", 0)]
        public void Parses_EnPassantFile(string fen, byte expected)
        {
            // Given
            // When
            var (board, _) = FenParser.Parse(fen);

            // Then
            Assert.Equal(expected, board.EnPassantFile);
        }
    }
}