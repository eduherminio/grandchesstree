using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;

namespace GrandChessTree.Toolkit
{
    public static class PositionsD4Generator
    {

        public static void GenerateD4HashFenDictionaryValues(string fileName)
        {
            var (board, whiteToMove) = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            board.Hash = Zobrist.CalculateZobristKey(ref board, true);
            var boards = LeafNodeGenerator.GenerateLeafNodes(ref board, 4, whiteToMove);

            var hashes = new HashSet<(ulong hash, string fen)>();
            Console.WriteLine($"{boards.Length} positions at d4");
            var collisionCount = 0;
            for (int i = 0; i < boards.Length; i++)
            {
                if (!hashes.Add((boards[i].Hash, boards[i].ToFen(true, 0, 1))))
                {
                    collisionCount++;
                }
            }

            Console.WriteLine($"{collisionCount} unique positions at d4");
            File.WriteAllLines(fileName, hashes.Select(h => $"[{h.hash}]=\"{h.fen}\","));
        }
    }
}
