using GrandChessTree.Client;

Console.WriteLine("-----TheGreatChessTree-----");

var workerCount = 8;  // Number of workers
var workerMemory = 1024; // Memory per worker (in MB)
var workerPath = "./GrandChessTree.Client.Worker.exe"; // Path to worker executable

Console.WriteLine($"Starting {workerCount} worker{(workerCount > 1 ? "s" : "")} with {workerMemory}MB of memory {(workerCount > 1 ? "each" : "")}");

// Create a list to store worker instances
var workers = new List<Worker>();
var commandList = new Queue<string>();
var (initialBoard, initialWhiteToMove) = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
var boards = MoveGenerator.PerftRoot(ref initialBoard, 1, initialWhiteToMove);
List<ulong> enqueuedFens = new List<ulong>();

Console.WriteLine($"Split search into {boards.Length} sub searches");
foreach (var board in boards)
{
    var fen = board.ToFen(!initialWhiteToMove);
    enqueuedFens.Add(board.Hash);
    var commandString = $"begin:8:{fen}";
    commandList.Enqueue(commandString);
}

// Initialize the workers
for (var i = 0; i < workerCount; i++)
{
    var worker = new Worker(workerPath, $"", i);
    workers.Add(worker);
    worker.Start();
}

var isRunning = true;

await Task.Delay(5000);
foreach (var worker in workers)
{
    worker.IsReady();
}

while (isRunning)
{
    foreach (var worker in workers)
    {
        if (worker.State == WorkerState.Ready)
        {
            if (commandList.Count > 0)
            {
                var nextFen = commandList.Dequeue();
                worker.NextTask(nextFen);
            }
        } 
    }

    if (workers.All(w => w.State == WorkerState.Ready))
    {
        Thread.Sleep(1000);
        if (workers.All(w => w.State == WorkerState.Ready))
        {
            isRunning = false;
        }
    }
    // Sleep a bit to avoid 100% CPU usage in the while loop
    Thread.Sleep(100);
}

Console.WriteLine("\nAll workers have completed.");

AggregateResultResult result = default;
foreach (var worker in workers)
{
    while (worker.WorkerResults.TryDequeue(out var workerResult))
    {
        result.Add(workerResult);
        enqueuedFens.Remove(workerResult.Hash);
        Console.WriteLine($"{workerResult.Hash} - {workerResult.Fen}");
    }
    
    worker.WaitForExit();
}

result.Nps /= boards.Length;
Console.WriteLine($"{enqueuedFens.Count} left..");
Console.WriteLine("---------------");
Console.WriteLine($"nps:{result.Nps}");
result.Print();
Console.WriteLine("---------------");

// Process the errors and output from workers
ProcessErrors(workers);
return;


static void ProcessErrors(List<Worker> workers)
{
    // Process the error logs
    foreach (var errorLine in workers.SelectMany(worker => worker.GetErrorLogs()))
    {
        Console.WriteLine(errorLine);  // Output error messages
    }
}