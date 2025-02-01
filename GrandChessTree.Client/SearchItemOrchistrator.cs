using System.Collections.Concurrent;
using System.Net.Http.Json;
using GrandChessTree.Shared;
using GrandChessTree.Shared.Api;
using GrandChessTree.Shared.Helpers;

namespace GrandChessTree.Client
{
    public class SearchItemOrchistrator
    {
        private readonly HttpClient _httpClient;

        public SearchItemOrchistrator(string apiUrl)
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri(apiUrl) };

            //foreach(var kvp in PositionsD4.Dict)
            //{
            //    _taskQueue.Enqueue(new D10SearchTaskResponse()
            //    {
            //        Depth = 3,
            //        SearchItemId = kvp.Key,
            //        Id = -1
            //    });
            //}
        }

        private ConcurrentQueue<D10SearchTaskResponse> _taskQueue = new ConcurrentQueue<D10SearchTaskResponse>();
        private SemaphoreSlim _taskLoaderLock = new SemaphoreSlim(1, 1); // Ensures only 1 thread loads

        public async Task<LocalSearchTask?> GetSearchItem()
        {
            if (!_taskQueue.TryDequeue(out var task))
            {
                await TryLoadNewTasks();

                if (!_taskQueue.TryDequeue(out task))
                {
                    return null;
                }
            }

            if(task == null)
            {
                return null;
            }

            if (!PositionsD4.Dict.TryGetValue(task.SearchItemId, out var position))
            {
                Console.Error.WriteLine($"Search item with hash {task.SearchItemId} not found. Waiting 30 seconds before retrying...");
                return null;
            }

            var searchTask = new LocalSearchTask();
            searchTask.Id = task.Id;
            searchTask.SearchItemId = task.SearchItemId;
            searchTask.SubTaskDepth = task.Depth;
            searchTask.SubTaskCount = 1;
            searchTask.Fen = position;
            searchTask.RemainingSubTasks.Add(position);


            //var (initialBoard, initialWhiteToMove) = FenParser.Parse(position);
            //var subTaskSplitDepth = 1;
            //var boards = LeafNodeGenerator.GenerateLeafNodes(ref initialBoard, subTaskSplitDepth, initialWhiteToMove);
            //var searchTask = new LocalSearchTask();
            //searchTask.Id = task.Id;
            //searchTask.SearchItemId = task.SearchItemId;
            //searchTask.SubTaskDepth = task.Depth - subTaskSplitDepth;
            //searchTask.SubTaskCount = boards.Length;
            //searchTask.Fen = position;
            //foreach (var board in boards)
            //{
            //    searchTask.RemainingSubTasks.Add(board.ToFen(!initialWhiteToMove, 0, 1));
            //}

            return searchTask;
        }

        private readonly ConcurrentQueue<SearchTaskResults> _completedResults = new();
        public void Submit(SearchTaskResults results)
        {
            _completedResults.Enqueue(results);
        }

        // Ensures only one thread loads new tasks
        private async Task TryLoadNewTasks()
        {
            await _taskLoaderLock.WaitAsync(); // Only one thread enters at a time

            try
            {
                if (_taskQueue.Any())
                {
                    return;
                }

                var tasks = await RequestNewTask(_httpClient);
                if (tasks == null || tasks.Length == 0)
                {
                    return; // No new tasks available
                }

                foreach (var newTask in tasks)
                {
                    _taskQueue.Enqueue(newTask);
                }
            }
            finally
            {
                _taskLoaderLock.Release(); // Allow next thread to proceed
            }
        }

        private static async Task<D10SearchTaskResponse[]?> RequestNewTask(HttpClient httpClient)
        {
            var response = await httpClient.PostAsync("api/v1/search/d10/tasks", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<D10SearchTaskResponse[]>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Console.Error.WriteLine("No available tasks at the moment.");
                await Task.Delay(TimeSpan.FromSeconds(10));
                return null;
            }

            Console.Error.WriteLine($"Failed to request new task. Status Code: {response.StatusCode}");
            return null;
        }

        public int Submitted { get; set; }
        public ulong TotalNodes { get; set; }
        public int PendingSubmission => _completedResults.Count;


        public async Task SubmitToApi()
        {
            var results = new List<SearchTaskResults>();
            while(_completedResults.Any() && results.Count < 200)
            {
                if (_completedResults.TryDequeue(out var res))
                {
                    results.Add(res);
                }
            }

            Submitted += results.Count();
            foreach (var result in results)
            {
                DuplicatesD4.Dict.TryGetValue(result.SearchItemId, out var occurrences);
                TotalNodes += result.Nodes * (ulong)occurrences;
            }

            if (results.Count == 0)
            {
                await Task.Delay(25);
                return;
            }

            var response = await _httpClient.PostAsJsonAsync($"api/v1/search/d10/tasks/results", new SearchTaskResultBatch()
            {
                Results = results.ToArray()
            });

            if (!response.IsSuccessStatusCode)
            {
                // Push back into completed task queue
                foreach (var result in results)
                {
                    _completedResults.Enqueue(result);
                }
            }

            await Task.Delay(25);
        }
    }
 }
