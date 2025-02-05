using System.Diagnostics;
using System.Xml.Linq;
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
        public NetworkClient(SearchItemOrchistrator searchItemOrchistrator, Config config)
        {
            _searchItemOrchistrator = searchItemOrchistrator;
            _workers = config.Workers;
            _workerReports = new WorkerReport[_workers];
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
            ulong prevTotalNodes = 0;
            long prevTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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
                    ulong sumCompletedSubTasks = 0;
                    float sumNps = 0;
                    long subtaskCacheHits = 0;

                    ulong sumNodes = 0;
                    ulong currentTotalNodes = 0;
                    ulong totalComputedNodes = 0;
                    for (int i = 0; i < _workerReports.Length; i++)
                    {
                        var report = _workerReports[i];
                        table.AddRow($"{i}", report.Nps.FormatBigNumber(), report.WorkerComputedNodes.FormatBigNumber(), $"{report.CompletedSubtasks}/{report.TotalSubtasks}", report.TotalCompletedTasks, report.Fen);
                        sumCompletedTasks += report.TotalCompletedTasks;
                        sumCompletedSubTasks += (ulong)report.TotalCompletedSubTasks;
                        subtaskCacheHits += report.TotalCachedSubTasks;
                        if (!float.IsNaN(report.Nps) && !float.IsInfinity(report.Nps))
                        {
                            sumNps += report.Nps;
                        }

                        currentTotalNodes += report.TotalNodes;
                        totalComputedNodes += report.TotalComputedNodes;
                    }

                    var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var realNps = (currentTotalNodes - prevTotalNodes) / ((currentTime - prevTime) / 1000f);

                    prevTotalNodes = currentTotalNodes;
                    prevTime = currentTime;

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

                    var cachHitPercent = sumCompletedSubTasks==0 ? 0 : ((float)subtaskCacheHits / sumCompletedSubTasks) * 100;
                    
                    Console.WriteLine($"completed {sumCompletedSubTasks.FormatBigNumber()} subtasks ({cachHitPercent.RoundToSignificantFigures(2)}% cache hits), submitted {_searchItemOrchistrator.Submitted} tasks ({_searchItemOrchistrator.PendingSubmission} pending)");
                    Console.WriteLine($"[computed stats] {totalComputedNodes.FormatBigNumber()} nodes at {sumNps.FormatBigNumber()}nps");
                    Console.WriteLine($"[effective stats] {currentTotalNodes.FormatBigNumber()} nodes at {realNps.FormatBigNumber()}nps ");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Thread.Sleep(500);

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
            Perft.HashTable = Perft.AllocateHashTable();

            var workerReport = _workerReports[index];
            while (IsRunning)
            {
                var searchItem = _searchItemOrchistrator.GetSearchItem();
                if (searchItem == null)
                {
                    Thread.Sleep(10);
                    continue;
                }
                if(!OccurrencesD4.Dict.TryGetValue(searchItem.PerftItemHash, out var searchItemOccurrences))
                {
                    searchItemOccurrences = 1;
                }
                workerReport.BeginTask(searchItem, searchItemOccurrences);

                long currentUnixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

                        var (fen, subTaskOccurrences) = subTask.Value;

                        workerReport.BeginSubTask(searchItem);

                        var (initialBoard, initialWhiteToMove) = FenParser.Parse(fen);

                        Summary summary = default;
                        if (_searchItemOrchistrator.SubTaskHashTable.TryGetValue(initialBoard.Hash, out summary))
                        {
                            workerReport.SubTaskCompletedFromCache(searchItem, summary.Nodes, searchItemOccurrences, subTaskOccurrences);
                        }
                        else
                        {
                            long subtaskStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                            Perft.PerftRoot(ref initialBoard, ref summary, searchItem.SubTaskDepth, initialWhiteToMove);
                            _searchItemOrchistrator.CacheCompletedSubtask(initialBoard.Hash, summary);
                            workerReport.EndSubTask(searchItem, summary.Nodes, searchItemOccurrences, subTaskOccurrences);
                            var subTaskDurationSeconds = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - subtaskStart) / 1000.0f;
                            workerReport.Nps = summary.Nodes / subTaskDurationSeconds;
                        }

                        searchItem.CompleteSubTask(summary, subTaskOccurrences);

                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error: {ex}");
                    }
                }

                if (searchItem.IsCompleted())
                {
                    var submission = searchItem.ToSubmission();
                    if (submission != null)
                    {
                        _searchItemOrchistrator.Submit(submission);
                        long duration = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - currentUnixTimeStamp;
                        if(duration <= 0)
                        {
                            duration = 1;
                        }

                        workerReport.CompleteTask(searchItem, duration);
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
