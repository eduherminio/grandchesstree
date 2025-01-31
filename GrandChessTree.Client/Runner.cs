using System.Diagnostics;
using ConsoleTables;
using GrandChessTree.Shared;
using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;
using Board = GrandChessTree.Shared.Board;

namespace GrandChessTree.Client
{
    public class Config
    {
        public int WorkerCount { get; set; } = Environment.ProcessorCount;
        public int WorkerMemory { get; set; } = 1024;
        public string WorkerPath { get; set; } = "./GrandChessTree.Client.Worker.exe";
        public int Depth { get; set; } = 9;
    }

    public class Runner
    {
        private readonly List<Worker> workers = new List<Worker>();
        private readonly List<WorkerResult> workerResults = new List<WorkerResult>();
        private readonly  Queue<string> commandList = new Queue<string>();
        private readonly Config _config;
        private bool HasQuit { get; set; } = false;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public AggregateResultResult result = new AggregateResultResult();

        private Board[] boards = [];
        public Runner(Config config) {
            _config = config;

            Console.WriteLine($"Starting {_config.WorkerCount} worker{(_config.WorkerCount > 1 ? "s" : "")} with {_config.WorkerMemory}MB of memory {(config.WorkerCount > 1 ? "each" : "")}");
            for (var i = 0; i < config.WorkerCount; i++)
            {
                var worker = new Worker(config.WorkerPath, $"", i);
                workers.Add(worker);
                worker.Start();
            }
        }

        public async Task Reset()
        {
            foreach (var worker in workers)
            {
                worker.Reset();
            }
        }

        public async Task Run(string rootFen)
        {
            commandList.Clear();
            workerResults.Clear();
            result = new AggregateResultResult();

            var (initialBoard, initialWhiteToMove) = FenParser.Parse(rootFen);

            if(_config.Depth <= 3)
            {
                var sw = Stopwatch.StartNew();
                var summary = Perft.PerftRoot(ref initialBoard, _config.Depth, initialWhiteToMove);
                var ms = sw.ElapsedMilliseconds;
                var s = (float)ms / 1000;
                var workerResult = new WorkerResult()
                {
                    Nps = (summary.Nodes / s),
                    Nodes = summary.Nodes,
                    Captures = summary.Captures,
                    Enpassant = summary.Enpassant,
                    Castles = summary.Castles,
                    Promotions = summary.Promotions,
                    DirectCheck = summary.DirectCheck,
                    SingleDiscoveredCheck = summary.SingleDiscoveredCheck,
                    DirectDiscoveredCheck = summary.DirectDiscoveredCheck,
                    DoubleDiscoveredCheck = summary.DoubleDiscoveredCheck,
                    DirectCheckmate = summary.DirectCheckmate,
                    SingleDiscoveredCheckmate = summary.SingleDiscoveredCheckmate,
                    DirectDiscoverdCheckmate = summary.DirectDiscoverdCheckmate,
                    DoubleDiscoverdCheckmate = summary.DoubleDiscoverdCheckmate,
                    Fen = rootFen,
                    Hash = Zobrist.CalculateZobristKey(ref initialBoard, initialWhiteToMove),
                };

                workerResults.Add(workerResult);
                result.Add(workerResult);
                PrintOutput();
                return;
            }

            boards = LeafNodeGenerator.GenerateLeafNodes(ref initialBoard, 2, initialWhiteToMove);

            Console.WriteLine($"Split search into {boards.Length} sub searches");
            foreach (var board in boards)
            {
                var fen = board.ToFen(!initialWhiteToMove);

                var commandString = $"begin:{_config.Depth - 2}:{fen}";
                commandList.Enqueue(commandString);
            }

            _stopwatch.Restart();
            Console.Clear();
            var iteration = 0;
            
            while (!HasQuit && (boards.Length != workerResults.Count))
            {
                foreach (var worker in workers)
                {
                    if (worker.State == WorkerState.Ready)
                    {
                        while (worker.WorkerResults.TryDequeue(out var workerResult))
                        {
                            workerResults.Add(workerResult);
                            result.Add(workerResult);
                        }

                        if (commandList.Count > 0)
                        {
                            var nextFen = commandList.Dequeue();
                            worker.NextTask(nextFen);
                        }
                    }
                }

                Thread.Sleep(50);
                if (iteration++ % 10 == 0)
                {
                    PrintReport();
                }
            }

            //Console.WriteLine(workerResults.DistinctBy(w => w.Hash).Count() + " / " + workerResults.Count());
            PrintOutput();
        }

        public void ProcessErrors()
        {
            // Process the error logs
            foreach (var errorLine in workers.SelectMany(worker => worker.GetErrorLogs()))
            {
                Console.WriteLine(errorLine);  // Output error messages
            }
        }

        void PrintReport()
        {
            var nps = result.Nps / workerResults.Count;
            var table = new ConsoleTable("key", "value");
            table
                .AddRow("depth", _config.Depth)
                .AddRow("workers", _config.WorkerCount)
                .AddRow("processed", $"{workerResults.Count}/{boards.Length}")
                .AddRow("single_nps", nps.FormatBigNumber())
                .AddRow("combined_nps", (nps * workers.Count()).FormatBigNumber())
                .AddRow("nodes", result.Nodes.FormatBigNumber())
                .AddRow("captures", result.Captures.FormatBigNumber())
                .AddRow("enpassants", result.Enpassant.FormatBigNumber())
                .AddRow("castles", result.Castles.FormatBigNumber())
                .AddRow("promotions", result.Promotions.FormatBigNumber())
                .AddRow("direct_checks", result.DirectCheck.FormatBigNumber())
                .AddRow("single_discovered_checks", result.SingleDiscoveredCheck.FormatBigNumber())
                .AddRow("direct_discovered_checks", result.DirectDiscoveredCheck.FormatBigNumber())
                .AddRow("double_discovered_check", result.DoubleDiscoveredCheck.FormatBigNumber())
                .AddRow("total_checks", (result.DirectCheck + result.SingleDiscoveredCheck + result.DirectDiscoveredCheck + result.DoubleDiscoveredCheck).FormatBigNumber())
                .AddRow("direct_mates", result.DirectCheckmate.FormatBigNumber())
                .AddRow("single_discovered_mates", result.SingleDiscoveredCheckmate.FormatBigNumber())
                .AddRow("direct_discoverd_mates", result.DirectDiscoverdCheckmate.FormatBigNumber())
                .AddRow("double_discoverd_mates", result.DoubleDiscoverdCheckmate.FormatBigNumber())
                .AddRow("total_mates", (result.DirectCheckmate + result.SingleDiscoveredCheckmate + result.DirectDiscoverdCheckmate + result.DoubleDiscoverdCheckmate).FormatBigNumber());

            table.Configure((c) =>
            {
                c.EnableCount = false;
            });

            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < Console.WindowHeight; y++)
                Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
            
            table.Write(Format.MarkDown);
        }


        void PrintOutput()
        {
            Console.Clear();
            var ms = _stopwatch.ElapsedMilliseconds;
            var s = (float)ms / 1000;
            var nps = result.Nps / workerResults.Count;

            if (commandList.Count == 0)
            {
                Console.WriteLine("\nAll workers have completed.");
            }
            else
            {
                Console.WriteLine($"\n{commandList.Count} unfinished tasks.");
            }
            Console.WriteLine("---------------");
            //Console.WriteLine($"fen:{initialBoard.ToFen(initialWhiteToMove)}");
            Console.WriteLine($"nps:{(result.Nodes / s).FormatBigNumber()}");
            Console.WriteLine($"nps:{(nps * workers.Count()).FormatBigNumber()}");
            result.Print();
            Console.WriteLine("---------------");
        }

        internal void Kill()
        {
            HasQuit = true;
            foreach (var worker in workers)
            {
                worker.Kill();
            }

            HasQuit = true;

            foreach (var worker in workers)
            {
                worker.WaitForExit();
            }
        }
    }
}
