using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;

namespace GrandChessTree.Toolkit
{
    public static class PositionsD4Generator
    {

        public static (Dictionary<ulong, string>, Dictionary<ulong, int>) GenerateD4HashFenDictionaryValues(string fileName)
        {
            var (initialBoard, whiteToMove) = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            var boards = LeafNodeGenerator.GenerateLeafNodes(ref initialBoard, 4, whiteToMove);

            var hashes = new Dictionary<ulong, string>();
            var duplicates = new Dictionary<ulong, int>();
            var collisionCount = 0;
            foreach(var board in boards)
            {
                hashes[board.Hash] = board.ToFen(true, 0, 1);

                if (duplicates.TryGetValue(board.Hash, out var count))
                {
                    count += 1;
                    collisionCount++;
                }
                else
                {
                    count = 1;
                }
                duplicates[board.Hash] = count;
            }

            Console.WriteLine($"total positions: {duplicates.Values.Sum()}");
            Console.WriteLine($"uniques {duplicates.Count}");
            Console.WriteLine($"duplicates {collisionCount}");

            File.WriteAllLines($"fens_{fileName}", hashes.Select(h => $"[{h.Key}]=\"{h.Value}\","));
            File.WriteAllLines($"duplicates_{fileName}", duplicates.Select(h => $"[{h.Key}]={h.Value},"));

            return (hashes, duplicates);
        }
    }
}
