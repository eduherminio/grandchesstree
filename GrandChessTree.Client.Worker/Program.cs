using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GrandChessTree.Client;
using GrandChessTree.Client.Tables;

if(args.Length == 0)
{
    //Console.WriteLine("No arguments passed.");
    //return;
}

RuntimeHelpers.RunClassConstructor(typeof(AttackTables).TypeHandle);
RuntimeHelpers.RunClassConstructor(typeof(Perft).TypeHandle);
ConcurrentQueue<string> CommandQueue = new();
ManualResetEventSlim CommandAvailable = new(false);
bool hasQuit = false;


_ = Task.Run(() =>
{
    ReadCommands();
});

Console.WriteLine("ready");
while (!hasQuit)
{
    CommandAvailable.Wait(); // Wait until a command is available
    CommandAvailable.Reset(); // Reset the event for the next wait

    while (CommandQueue.TryDequeue(out var command))
    {
        if (command.StartsWith("reset"))
        {
            Console.WriteLine("ready");
        }
        else if (command.StartsWith("begin"))
        {
            var commandParts = command.Split(":");
            if (int.TryParse(commandParts[1], out var depth))
            {
                var board = FenParser.Parse(commandParts[2]);
                var sw = Stopwatch.StartNew();
                Summary summary = Perft.PerftRoot(ref board, depth, true);
                var ms = sw.ElapsedMilliseconds;
                var s = (float)ms / 1000;
                Console.WriteLine($"nps:{(summary.Nodes / s)}");
                summary.Print();
                Console.WriteLine("done");
            }
        }
    }
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
            hasQuit = true;
            Environment.Exit(0);
            break;
        }

        CommandQueue.Enqueue(command);
        CommandAvailable.Set(); // Signal that a command is available
    }
}