using GreatPerft;

namespace Tests
{
    public class PerftTests
    {
        [Theory]
        [InlineData(0, 1, 0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(1, 20, 0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(2, 400, 0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(3, 8_902, 34, 0, 0, 0, 12, 0, 0, 0)]
        [InlineData(4, 197_281, 1576, 0, 0, 0, 469, 0, 0, 8)]
        [InlineData(5, 4_865_609, 82_719, 258, 0, 0, 27_351, 0, 0, 347)]
        public void InitialPosition_Perft_Returns_Expected_Summary(byte depth, ulong nodes, ulong captures, ulong enpassant, ulong castles, ulong promotions, ulong checks, ulong discoveryChecks, ulong doubleChecks, ulong checkMates)
        {
            // Given
            var board = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Summary summary = default;

            // When
            Perft.PerftWhite(ref board, ref summary, depth);

            // Then
            Assert.Equal(nodes, summary.Nodes);
            Assert.Equal(captures, summary.Captures);
            Assert.Equal(enpassant, summary.Enpassant);
            Assert.Equal(castles, summary.Castles);
            Assert.Equal(promotions, summary.Promotions);
            Assert.Equal(checks, summary.Checks);
            Assert.Equal(discoveryChecks, summary.DiscoveryChecks);
            Assert.Equal(doubleChecks, summary.DoubleChecks);
            Assert.Equal(checkMates, summary.CheckMates);
        }

        [Theory]
        [InlineData(1, 48, 8, 0, 2, 0, 0, 0, 0, 0)]
        [InlineData(2, 2039, 351, 1, 91, 0, 3, 0, 0, 0)]
        [InlineData(3, 97862, 17102, 45, 3162, 0, 993, 0, 0, 1)]
        [InlineData(4, 4_085_603, 757163, 1929, 128013, 15172, 25523, 0, 0, 43)]
        public void Position2_Perft_Returns_Expected_Summary(byte depth, ulong nodes, ulong captures, ulong enpassant, ulong castles, ulong promotions, ulong checks, ulong discoveryChecks, ulong doubleChecks, ulong checkMates)
        {
            // Given
            var board = FenParser.Parse("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -");
            Summary summary = default;

            // When
            Perft.PerftWhite(ref board, ref summary, depth);

            // Then
            Assert.Equal(nodes, summary.Nodes);
            Assert.Equal(captures, summary.Captures);
            Assert.Equal(enpassant, summary.Enpassant);
            Assert.Equal(castles, summary.Castles);
            Assert.Equal(promotions, summary.Promotions);
            Assert.Equal(checks, summary.Checks);
            Assert.Equal(discoveryChecks, summary.DiscoveryChecks);
            Assert.Equal(doubleChecks, summary.DoubleChecks);
            Assert.Equal(checkMates, summary.CheckMates);
        }

        [Theory]
        [InlineData(1, 14, 1, 0, 0, 0, 2, 0, 0, 0)]
        [InlineData(2, 191, 14, 0, 0, 0, 10, 0, 0, 0)]
        [InlineData(3, 2812, 209, 2, 0, 0, 267, 0, 0, 0)]
        [InlineData(4, 43238, 3348, 123, 0, 0, 1680, 0, 0, 17)]
        [InlineData(5, 674624, 52051, 1165, 0, 0, 52950, 0, 0, 0)]
        [InlineData(6, 11_030_083, 940350, 33325, 0, 7552, 452473, 0, 0, 2733)]
        public void Position3_Perft_Returns_Expected_Summary(byte depth, ulong nodes, ulong captures, ulong enpassant, ulong castles, ulong promotions, ulong checks, ulong discoveryChecks, ulong doubleChecks, ulong checkMates)
        {
            // Given
            var board = FenParser.Parse("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
            Summary summary = default;

            // When
            Perft.PerftWhite(ref board, ref summary, depth);

            // Then
            Assert.Equal(nodes, summary.Nodes);
            Assert.Equal(captures, summary.Captures);
            Assert.Equal(enpassant, summary.Enpassant);
            Assert.Equal(castles, summary.Castles);
            Assert.Equal(promotions, summary.Promotions);
            Assert.Equal(checks, summary.Checks);
            Assert.Equal(discoveryChecks, summary.DiscoveryChecks);
            Assert.Equal(doubleChecks, summary.DoubleChecks);
            Assert.Equal(checkMates, summary.CheckMates);
        }

        [Theory]
        [InlineData(1, 6, 0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(2, 264, 87, 0, 6, 48, 10, 0, 0, 0)]
        [InlineData(3, 9467, 1021, 4, 0, 120, 38, 0, 0, 22)]
        [InlineData(4, 422333, 131393, 0, 7795, 60032, 15492, 0, 0, 5)]
        [InlineData(5, 15_833_292, 2046173, 6512, 0, 329464, 200568, 0, 0, 50562)]
        public void Position4_Perft_Returns_Expected_Summary(byte depth, ulong nodes, ulong captures, ulong enpassant, ulong castles, ulong promotions, ulong checks, ulong discoveryChecks, ulong doubleChecks, ulong checkMates)
        {
            // Given
            var board = FenParser.Parse("r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ - 0 1 \r\n");
            Summary summary = default;

            // When
            Perft.PerftBlack(ref board, ref summary, depth);

            // Then
            Assert.Equal(nodes, summary.Nodes);
            Assert.Equal(captures, summary.Captures);
            Assert.Equal(enpassant, summary.Enpassant);
            Assert.Equal(castles, summary.Castles);
            Assert.Equal(promotions, summary.Promotions);
            Assert.Equal(checks, summary.Checks);
            Assert.Equal(discoveryChecks, summary.DiscoveryChecks);
            Assert.Equal(doubleChecks, summary.DoubleChecks);
            Assert.Equal(checkMates, summary.CheckMates);
        }
    }
}