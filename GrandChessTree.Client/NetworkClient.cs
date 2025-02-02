using System.Diagnostics;
using ConsoleTables;
using GrandChessTree.Shared;
using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;

namespace GrandChessTree.Client
{

    public class NetworkClient
{
        private readonly int _workers;
        private readonly SearchItemOrchistrator _searchItemOrchistrator;
        public bool IsRunning { get; set; } = true;
        public WorkerReport[] _workerReports;
        public NetworkClient(SearchItemOrchistrator searchItemOrchistrator, int workers)
        {
            _searchItemOrchistrator = searchItemOrchistrator;
            _workers = workers;
            _workerReports = new WorkerReport[workers];
            for(int i = 0; i < _workerReports.Length; i++) { _workerReports[i] = new WorkerReport(); }
        }

        public async Task RunMultiple()
        {
            var tasks = new List<Task>
            {
                OutputStatsPeriodically(),
                SubmitResultsPeriodically()
            };

            // Start the tasks concurrently
            for (int i = 0; i < _workers; i++)
            {
                tasks.Add(Run(i));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
        }

        private async Task OutputStatsPeriodically()
        {
            while (IsRunning)
            {
                try
                {
                    Console.CursorVisible = false;
                    Console.SetCursorPosition(0, 0);
                    for (int y = 0; y < Console.WindowHeight; y++)
                        Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, 0);

                    Console.WriteLine();

                    var table = new ConsoleTable("worker", "nps", "nodes", "sub_tasks", "tasks", "fen");

                    var sumCompletedTasks = 0;
                    var sumCompletedSubTasks = 0;
                    float sumNps = 0;
                    for (int i = 0; i < _workerReports.Length; i++)
                    {
                        var report = _workerReports[i];
                        table.AddRow($"{i}", report.Nps.FormatBigNumber(), report.Nodes.FormatBigNumber(), $"{report.CompletedSubtasks}/{report.TotalSubtasks}", report.TotalCompletedTasks, report.Fen);
                        sumCompletedTasks += report.TotalCompletedTasks;
                        sumCompletedSubTasks += report.TotalCompletedSubTasks;
                        sumNps += report.Nps;
                    }

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

                    Console.WriteLine($"completed {sumCompletedSubTasks} subtasks, submitted {_searchItemOrchistrator.Submitted} ({_searchItemOrchistrator.PendingSubmission} pending) tasks at {sumNps.FormatBigNumber()}nps, {_searchItemOrchistrator.TotalNodes} nodes ");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                               
                await Task.Delay(250);

            }
        }

        private async Task SubmitResultsPeriodically()
        {
            while (IsRunning)
            {
                try
                {
                    await _searchItemOrchistrator.SubmitToApi();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        public async Task Run(int index)
        {
            var sw = Stopwatch.StartNew();
            var hashTable = new Summary[Perft.HashTableSize];


            while (IsRunning)
            {
                var searchItem = await _searchItemOrchistrator.GetSearchItem();
                if (searchItem == null)
                {
                    await Task.Delay(100);
                    continue;
                }
                var span = hashTable.AsSpan();
                while (IsRunning && searchItem.RemainingSubTasks.Any())
                {
                    try
                    {
                        var position = searchItem.GetNextSubTask();
                        if(position == null)
                        {
                            continue;
                        }
                        _workerReports[index].BeginSubTask(searchItem);

                        var (initialBoard, initialWhiteToMove) = FenParser.Parse(position);

                        Summary summary = default;
                        sw.Restart();
                        Perft.PerftRoot(span, ref initialBoard, ref summary, searchItem.SubTaskDepth, initialWhiteToMove);
                        var ms = sw.ElapsedMilliseconds;
                        var seconds = sw.ElapsedTicks / (float)Stopwatch.Frequency;
                        var result = new WorkerResult()
                        {
                            Nps = seconds > 0 ? (summary.Nodes / seconds) : summary.Nodes,
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
                            Fen = position,
                            Hash = initialBoard.Hash,
                        };

                        searchItem.CompleteSubTask(result);
                        _workerReports[index].EndSubTask(searchItem, result.Nps);

                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error: {ex.Message}");
                    }
                }

                if (searchItem.IsCompleted())
                {
                    var submission = searchItem.ToSubmission();
                    if(submission != null)
                    {
                        _searchItemOrchistrator.Submit(submission);
                        _workerReports[index].CompleteTask(searchItem);
                    }
                    else
                    {
                        Console.Error.WriteLine($"Error: failed to parse submission...");
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Error: incompleted task...");
                }
            }
            
        }
}
 }
