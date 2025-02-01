using GrandChessTree.Client;

Console.WriteLine("-----TheGreatChessTree-----");


Console.WriteLine("Enter the api url:");
var apiUrl = Console.ReadLine();
while (string.IsNullOrEmpty(apiUrl))
{
    Console.WriteLine("Please enter a valid url");
}


var networkClient = new NetworkClient(apiUrl, 6, 30);
AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
_ = Task.Run(ReadCommands);

await networkClient.RunMultiple();

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