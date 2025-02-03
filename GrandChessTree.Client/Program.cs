using GrandChessTree.Client;

Console.WriteLine("-----TheGreatChessTree-----");

var config = ConfigManager.LoadOrCreateConfig();

if (!ConfigManager.IsValidConfig(config))
{
    return;
}

Console.WriteLine("Enter the perft depth to start:");
if (!int.TryParse(Console.ReadLine(), out int depth) || depth <= 4)
{
    Console.WriteLine("Invalid search depth");
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