using System.Diagnostics;
using GrandChessTree.Shared;
using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;
using static System.Net.Mime.MediaTypeNames;

namespace GrandChessTree.Toolkit
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("----The Grand Chess Tree Toolkit----");
            //Test();
            //await PositionD10Seeder.SeedD10();
            PositionsD4Generator.GenerateD4HashFenDictionaryValues("out.cs");
        }

        public static void Test()
        {
            ulong nodes = 0;

            //var (initialBoard, whiteToMove) = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            //initialBoard.Hash = Zobrist.CalculateZobristKey(ref initialBoard, true);
            //var boards = LeafNodeGenerator.GenerateLeafNodes(ref initialBoard, 4, whiteToMove);

            var (hashes, duplicates) = PositionsD4Generator.GenerateD4HashFenDictionaryValues("sd");
            var collisionCount = 0;
            var hashTable = new Summary[Perft.HashTableSize];

            var cnt = 0;
            foreach (var kvp in PositionsD4.Dict)
            {
                Summary summary = default;
                var (b, initialWhiteToMove) = FenParser.Parse(kvp.Value);

                Perft.PerftRoot(hashTable, ref b, ref summary, 2, initialWhiteToMove);
                nodes += summary.Nodes * (ulong)DuplicatesD4.Dict[kvp.Key];
                Console.WriteLine($"total: {nodes} {cnt++}");
            }


            //Console.WriteLine(PositionsD4.Dict.Count());
            //Console.ReadLine();
            //foreach(var kvp in PositionsD4.Dict)
            //{
            //    if(kvp.Key == 18426721153457163832 || kvp.Key == 267725829108852316 || kvp.Key == 7791023388153502912 || kvp.Key == 10381003873507685540)
            //    {
            //        continue;
            //    }

            //    var (initialBoard, initialWhiteToMove) = FenParser.Parse(kvp.Value);

            //    Summary summary = default;
            //    Perft.PerftRoot(ref initialBoard, ref summary, 2, true);
            //    nodes += summary.Nodes * (ulong)DuplicatesD4.Dict[kvp.Key];
            //    count++;
            //}
        }
    }
}
