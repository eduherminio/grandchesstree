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
        private static int _searchDepth;
        private readonly Config _config;
        public SearchItemOrchistrator(int searchDepth, Config config)
        {
            _config = config;
            _searchDepth = searchDepth;
            _httpClient = new HttpClient() { BaseAddress = new Uri(config.ApiUrl) };
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);
        }

        private ConcurrentQueue<PerftTaskResponse> _taskQueue = new ConcurrentQueue<PerftTaskResponse>();
        public int Submitted { get; set; }
        public ulong TotalNodes { get; set; }
        public int PendingSubmission => _completedResults.Count;

        public PerftTask? GetSearchItem()
        {
            if (!_taskQueue.TryDequeue(out var task))
            {
                return null;
            }

            if (task == null)
            {
                return null;
            }

            if (!PositionsD4.Dict.TryGetValue(task.PerftItemHash, out var position))
            {
                Console.Error.WriteLine($"Search item with hash {task.PerftItemHash} not found. Waiting 30 seconds before retrying...");
                return null;
            }

            //var searchTask = new LocalSearchTask();
            //searchTask.Id = task.Id;
            //searchTask.SearchItemId = task.SearchItemId;
            //searchTask.SubTaskDepth = task.Depth;
            //searchTask.SubTaskCount = 1;
            //searchTask.Fen = position;
            //searchTask.RemainingSubTasks.Add(position);

            var (initialBoard, initialWhiteToMove) = FenParser.Parse(position);

            var subTaskSplitDepth = 2;
            var fens = LeafNodeGenerator.GenerateLeafNodes(ref initialBoard, subTaskSplitDepth, initialWhiteToMove);
            var searchTask = new PerftTask() {
                PerftTaskId = task.PerftTaskId,
                PerftItemHash = task.PerftItemHash,
                SubTaskDepth = task.Depth - 4 - subTaskSplitDepth,
                SubTaskCount = fens.Count(),
                Fen = position,
                RemainingSubTasks = fens
            };
   
            return searchTask;
        }

        private readonly ConcurrentQueue<PerftTaskResult> _completedResults = new();
        public void Submit(PerftTaskResult results)
        {
            _completedResults.Enqueue(results);
        }

        // Ensures only one thread loads new tasks
        public async Task<bool> TryLoadNewTasks()
        {
            if (_taskQueue.Count() >= _config.Workers)
            {
                return false;
            }

            var tasks = await RequestNewTask(_httpClient);
            if (tasks == null || tasks.Length == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                return false; // No new tasks available
            }

            foreach (var newTask in tasks)
            {
                _taskQueue.Enqueue(newTask);
            }

            return true;
        }

        private static async Task<PerftTaskResponse[]?> RequestNewTask(HttpClient httpClient)
        {
            var response = await httpClient.PostAsync($"api/v1/perft/{_searchDepth}/tasks", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PerftTaskResponse[]>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.Error.WriteLine("No available tasks at the moment.");
                await Task.Delay(TimeSpan.FromSeconds(10));
                return null;
            }

            Console.Error.WriteLine($"Failed to request new task. Status Code: {response.StatusCode}");
            return null;
        }

        public async Task<bool> SubmitToApi()
        {
            var results = new List<PerftTaskResult>();
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
                OccurrencesD4.Dict.TryGetValue(result.PerftItemHash, out var occurrences);
                TotalNodes += result.Nodes * (ulong)occurrences;
            }

            if (results.Count == 0)
            {
                return false;
            }

            var response = await _httpClient.PostAsJsonAsync($"api/v1/perft/{_searchDepth}/results", new PerftTaskResultBatch()
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
                await Task.Delay(TimeSpan.FromSeconds(10));
                return false;
            }

            await Task.Delay(100);
            return true;
        }
    }
 }
