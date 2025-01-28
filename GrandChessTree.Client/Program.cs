using ConsoleTables;
using GrandChessTree.Client;


Console.WriteLine("-----TheGreatChessTree-----");

var config = new Config()
{
    WorkerCount = 32,
    Depth = 10,
};

var fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

var runner = new Runner(config);

AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
_ = Task.Run(ReadCommands);
await runner.Run(fen);

//var results = new List<AggregateResultResult>();
//for(int i = 4; i < 9; i++)
//{
//    config.Depth = i;
//    await runner.Run(fen);
//    results.Add(runner.result);
//    await runner.Reset();
//    Console.WriteLine("depth: " +  i);
//    await Task.Delay(500);
//}

//var table = new ConsoleTable("depth", "nodes", "captures", "enpassants", "castles", "promotions", "checks", "discovered_checks", "double_checks", "check_mates");
//for (int i = 0; i < results.Count; i++)
//{
//    var result = results[i];
//    table.AddRow(i + 4, result.Nodes, result.Captures, result.Enpassant, result.Castles, result.Promotions, result.Checks, result.DiscoveredChecks, result.DoubleChecks, result.CheckMates);
//}

//table.Configure((c) =>
//{
//    c.EnableCount = false;
//});

//Console.Clear();
//table.Write();

Console.ReadLine();

void CurrentDomain_ProcessExit(object? sender, EventArgs e)
{
    Console.WriteLine("process exited");
    runner.Kill();
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
        if (command.Contains("quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Quitting");
            runner.Kill();
            Environment.Exit(0);
            break;
        }
    }
}