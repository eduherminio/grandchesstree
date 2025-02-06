using GrandChessTree.Client;

Console.WriteLine("-----TheGreatChessTree-----");
var containerized = Environment.GetEnvironmentVariable("containerized");

int depth = 0;
Config config;
// Check if it's set and print it out (or use it in your logic)
if (containerized != null && containerized == "true")
{
    Console.WriteLine($"Running in container");

    var workerEnvVar = Environment.GetEnvironmentVariable("workers");
    if(!int.TryParse(workerEnvVar, out var workerCount))
    {
        Console.WriteLine("'worker' environment variable must be an integer > 0");
        return;
    }

    var depthEnvVar = Environment.GetEnvironmentVariable("depth");
    if (!int.TryParse(depthEnvVar, out depth))
    {
        Console.WriteLine("'depth' environment variable must be an integer > 0");
        return;
    }

    config = new Config()
    {
        ApiKey = Environment.GetEnvironmentVariable("api_key") ?? "",
        ApiUrl = Environment.GetEnvironmentVariable("api_url") ?? "",
        Workers = workerCount,
    };
}
else
{
    config = ConfigManager.LoadOrCreateConfig();
    Console.WriteLine("Enter the perft depth to start:");
    if (!int.TryParse(Console.ReadLine(), out depth))
    {
        Console.WriteLine("Invalid search depth");
    }
}


if (!ConfigManager.IsValidConfig(config))
{
    return;
}


var searchOrchastrator = new SearchItemOrchistrator(depth, config);
var networkClient = new NetworkClient(searchOrchastrator, config);
AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
_ = Task.Run(ReadCommands);

networkClient.RunMultiple();

void CurrentDomain_ProcessExit(object? sender, EventArgs e)
{
    Console.WriteLine("process exited");
    networkClient.IsRunning = false;
}

void ReadCommands()
{
    while (true)
    {
        var command = Console.ReadLine();
        if (string.IsNullOrEmpty(command))
        {
            continue; // Skip empty commands
        }

        command = command.Trim();
        if (command.ToLower() == "q")
        {
            Environment.Exit(0);
            break;
        }
    }
}