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

var results = new List<AggregateResultResult>();

await runner.Run(fen);
results.Add(runner.result);

var startDepth = 10;
//for (int i = startDepth; i <= 8; i++)
//{
//    config.Depth = i;
//    await runner.Run(fen);
//    results.Add(runner.result);
//    await runner.Reset();
//    Console.WriteLine("depth: " + i);
//    await Task.Delay(500);
//}

var table = new ConsoleTable("depth", "nodes", "captures", "enpassants", "castles", "promotions",
    "direct_checks", "single_discovered_checks", "direct_discovered_checks", "double_discovered_check",
    "total_checks", "direct_mates", "single_discovered_mates", "direct_discoverd_mates", "double_discoverd_mates", "total_mates");
for (int i = 0; i < results.Count; i++)
{
    var result = results[i];
    table.AddRow(i + startDepth, 
        result.Nodes, result.Captures, result.Enpassant, result.Castles, result.Promotions, 
        result.DirectCheck, result.SingleDiscoveredCheck, result.DirectDiscoveredCheck, result.DoubleDiscoveredCheck, (result.DirectCheck + result.SingleDiscoveredCheck + result.DirectDiscoveredCheck + result.DoubleDiscoveredCheck),
        result.DirectCheckmate, result.SingleDiscoveredCheckmate, result.DirectDiscoverdCheckmate, result.DoubleDiscoverdCheckmate,(result.DirectCheckmate + result.SingleDiscoveredCheckmate + result.DirectDiscoverdCheckmate + result.DoubleDiscoverdCheckmate)
        );
}

table.Configure((c) =>
{
    c.EnableCount = false;
});

Console.Clear();
table.Write(Format.MarkDown);

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