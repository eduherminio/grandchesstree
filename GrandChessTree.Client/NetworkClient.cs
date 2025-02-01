using System.Diagnostics;
using System.Net.Http.Json;
using System.Threading;
using ConsoleTables;
using GrandChessTree.Client;
using GrandChessTree.Shared;
using GrandChessTree.Shared.Api;
using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;

namespace GrandChessTree.Client
{
    public class NetworkClient
{
        private readonly HttpClient _httpClient;
        private readonly int _depth;
        private readonly int _workers;

        public bool IsRunning { get; set; } = true;

        public Summary[] Results { get; set; }
        public string[] Fens { get; set; }
        public float[] Nps { get; set; }

        public int TotalTasks { get; set; } = 0;
        public NetworkClient(string apiUrl, int depth, int workers)
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri(apiUrl) };
            _depth = depth;
            _workers = workers;
            Results = new Summary[workers];
            Fens = new string[workers];
            Nps = new float[workers];
        }

        public async Task RunMultiple()
        {
            var tasks = new List<Task>();
            // Create a periodic task that outputs stats every 1 second

            // Start the tasks concurrently
            for (int i = 0; i < _workers; i++)
            {
                tasks.Add(Run(i));
            }
            tasks.Add(OutputStatsPeriodically());

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
        }

        private async Task OutputStatsPeriodically()
        {
            while (IsRunning)
            {
                Console.CursorVisible = false;
                Console.SetCursorPosition(0, 0);
                for (int y = 0; y < Console.WindowHeight; y++)
                    Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, 0);


                Console.WriteLine();

                 var table = new ConsoleTable("key", "value");
                table
                    .AddRow("depth", _depth)
                    .AddRow("workers", _workers)
                    .AddRow("avg_nps", (Nps.Sum() / _workers).FormatBigNumber())
                    .AddRow("total_nps", (Nps.Sum()).FormatBigNumber())
                    .AddRow("completed", TotalTasks);

                for(int i = 0; i < Results.Length; i++)
                {
                    table.AddRow($"fen({i})", Fens[i]);
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

                await Task.Delay(100);
            }
        }

        public async Task Run(int index)
        {
            while (IsRunning)
            {
                try
                {
                    var task = await RequestNewTask(_httpClient);
                    if (task == null)
                    {
                        Console.Error.WriteLine("No available tasks. Waiting 30 seconds before retrying...");
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        continue;
                    }

                    if (!PositionsD4.Dict.TryGetValue(task.SearchItemId, out var position))
                    {
                        Console.Error.WriteLine($"Search item with hash {task.SearchItemId} not found. Waiting 30 seconds before retrying...");
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        continue;
                    }

                    Fens[index] = position;
                    var (initialBoard, initialWhiteToMove) = FenParser.Parse(position);
                    var result = new AggregateResultResult();

                    var sw = Stopwatch.StartNew();
            
                    Results[index] = default;
                    Perft.PerftRoot(ref initialBoard, ref Results[index], _depth, initialWhiteToMove);
                    var ms = sw.ElapsedMilliseconds;
                    var s = (float)ms / 1000;
                    var summary = Results[index];
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
                        Fen = position,
                        Hash = Zobrist.CalculateZobristKey(ref initialBoard, initialWhiteToMove),
                    };

                    TotalTasks++;
                    Nps[index] = workerResult.Nps;
                    result.Add(workerResult);
                    //OutputHelpers.PrintReport(position, _depth, result);

                    bool resultSubmitted = await SubmitResult(_httpClient, task.Id, result);
                    if (!resultSubmitted)
                    {
                        Console.Error.WriteLine($"Failed to submit result for Task {task.Id}");
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }

                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
            }
        }



    // Request a new search task
    private static async Task<D10SearchTaskResponse?> RequestNewTask(HttpClient httpClient)
    {
        var response = await httpClient.PostAsync("api/v1/search/d10/tasks", null);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<D10SearchTaskResponse>();
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            Console.Error.WriteLine("No available tasks at the moment.");
            return null;
        }

        Console.Error.WriteLine($"Failed to request new task. Status Code: {response.StatusCode}");
        return null;
    }

    // Submit a fake result for a given task ID
    private static async Task<bool> SubmitResult(HttpClient httpClient, int taskId, AggregateResultResult result)
    {
        var request = new SubmitSearchTaskResultRequest()
        {
            Nodes = result.Nodes,
            Captures = result.Captures,
            Enpassant = result.Enpassant,
            Castles = result.Castles,
            Promotions = result.Promotions,
            DirectCheck = result.DirectCheck,
            SingleDiscoveredCheck = result.SingleDiscoveredCheck,
            DirectDiscoveredCheck = result.DirectDiscoveredCheck,
            DoubleDiscoveredCheck = result.DoubleDiscoveredCheck,
            DirectCheckmate = result.DirectCheckmate,
            SingleDiscoveredCheckmate = result.SingleDiscoveredCheckmate,
            DirectDiscoverdCheckmate = result.DirectDiscoverdCheckmate,
            DoubleDiscoverdCheckmate = result.DoubleDiscoverdCheckmate,
        };

        var response = await httpClient.PostAsJsonAsync($"api/v1/search/d10/tasks/{taskId}/result", request);
        return response.IsSuccessStatusCode;
    }
}
 }

public static class OutputHelpers
{
    public static void PrintReport(string fen, int depth, AggregateResultResult result)
    {
        
        var table = new ConsoleTable("key", "value");
        table
            .AddRow("fen", fen)
            .AddRow("depth", depth)
            .AddRow("nps", result.Nps.FormatBigNumber())
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


}