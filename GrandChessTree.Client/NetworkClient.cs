using System.Diagnostics;
using ConsoleTables;
using GrandChessTree.Shared;
using GrandChessTree.Shared.Helpers;

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

        public void RunMultiple()
        {
            Thread[] threads = new Thread[_workers + 3];

            for (int i = 0; i < _workers; i++)
            {
                var index = i;
                threads[index] = new Thread(() => ThreadWork(index));
                threads[index].Start();
            }

            threads[_workers] = new Thread(OutputStatsPeriodically);
            threads[_workers].Start();

            threads[_workers + 1] = new Thread(GetTasksPeriodically);
            threads[_workers + 1].Start();   
            
            threads[_workers + 2] = new Thread(SubmitResultsPeriodically);
            threads[_workers + 2].Start();

            // Wait for all threads to complete
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        private void OutputStatsPeriodically()
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

                Thread.Sleep(250);

            }
        }

        public void GetTasksPeriodically()
        {
            Task.Run(async () =>
            {
                while (IsRunning)
                {
                    try
                    {
                        await _searchItemOrchistrator.TryLoadNewTasks();
                        await Task.Delay(10);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            });
        }

        public void SubmitResultsPeriodically()
        {
            Task.Run(async () =>
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
                    await Task.Delay(100);
                }

            });
        }

        private unsafe void ThreadWork(int index)
        {
            var sw = Stopwatch.StartNew();
            Perft.HashTable = Perft.AllocateHashTable();

            while (IsRunning)
            {
                var searchItem = _searchItemOrchistrator.GetSearchItem();
                if (searchItem == null)
                {
                    Thread.Sleep(10);
                    continue;
                }
                while (IsRunning && searchItem.RemainingSubTasks.Any())
                {
                    try
                    {
                        var subTask = searchItem.GetNextSubTask();
                        if (subTask == null)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        var (fen, occurrences) = subTask.Value;

                        _workerReports[index].BeginSubTask(searchItem);

                        var (initialBoard, initialWhiteToMove) = FenParser.Parse(fen);

                        Summary summary = default;
                        sw.Restart();
                        Perft.PerftRoot(ref initialBoard, ref summary, searchItem.SubTaskDepth, initialWhiteToMove);
                        var ms = sw.ElapsedMilliseconds;
                        var seconds = sw.ElapsedTicks / (float)Stopwatch.Frequency;
                        var result = new WorkerResult()
                        {
                            Nps = seconds > 0 ? (summary.Nodes * (ulong)occurrences / seconds) : 0,
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
                            Fen = fen,
                            Hash = initialBoard.Hash,
                        };

                        searchItem.CompleteSubTask(result, occurrences);
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
                    if (submission != null)
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
