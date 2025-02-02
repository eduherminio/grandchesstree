using GrandChessTree.Client;

Console.WriteLine("-----TheGreatChessTree-----");


var apiUrl = "http://10.0.3.122:5032/";
Console.WriteLine("Hit enter to start");
Console.ReadLine();


var searchOrchastrator = new SearchItemOrchistrator(apiUrl);
var networkClient = new NetworkClient(searchOrchastrator, 30);
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