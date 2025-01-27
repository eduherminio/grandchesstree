Console.WriteLine("-----TheGreatChessTree-----");

var workerCount = 4;  // Number of workers
var workerMemory = 1024; // Memory per worker (in MB)
var workerPath = "./GrandChessTree.Client.Worker.exe"; // Path to worker executable

Console.WriteLine($"Starting {workerCount} worker{(workerCount > 1 ? "s" : "")} with {workerMemory}MB of memory {(workerCount > 1 ? "each" : "")}");

// Create a list to store worker instances
var workers = new List<Worker>();
var commandList = new Queue<string>();
var fenString = "begin:5:8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -"; // Initial FEN (change as needed)

for (int i = 0; i < 20; i++)
{
    commandList.Enqueue(fenString);
}

// Initialize the workers
for (int i = 0; i < workerCount; i++)
{
    var worker = new Worker(workerPath, $"", i);
    workers.Add(worker);
    worker.Start();
}

// Start the process of updating the FEN string when a worker is done
var finishedWorkers = new List<Worker>();
bool isRunning = true;

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

        if (worker.WorkerResults.TryDequeue(out var result))
        {
            Console.WriteLine($"new result! {result.Nps}nps");
        }
    }

    // Sleep a bit to avoid 100% CPU usage in the while loop
    Thread.Sleep(100);
}

Console.WriteLine("\nAll workers have completed.");

// Process the errors and output from workers
ProcessErrors(workers);
    

    static void ProcessErrors(List<Worker> workers)
{
    // Process the error logs
    foreach (var worker in workers)
    {
        foreach (var errorLine in worker.GetErrorLogs())
        {
            Console.WriteLine(errorLine);  // Output error messages
        }
    }
}