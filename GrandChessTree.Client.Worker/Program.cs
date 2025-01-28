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

//// test
//var (board, whiteToMove) = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
//var sw = Stopwatch.StartNew();
//var summary = Perft.PerftRoot(ref board, 7, whiteToMove);
//var ms = sw.ElapsedMilliseconds;
//var s = (float)ms / 1000;
//Console.WriteLine($"fen:{board.ToFen(whiteToMove)}");
//Console.WriteLine($"nps:{(summary.Nodes / s).FormatBigNumber()}");
//summary.Print();
//Console.WriteLine("done");
//// test
//return;

ConcurrentQueue<string> commandQueue = new();
using ManualResetEventSlim commandAvailable = new(false);
var hasQuit = false;


_ = Task.Run(ReadCommands);

Console.WriteLine("ready");
while (!hasQuit)
{
    commandAvailable.Wait(); // Wait until a command is available
    commandAvailable.Reset(); // Reset the event for the next wait

    while (commandQueue.TryDequeue(out var command))
    {
        if (command.StartsWith("reset"))
        {
            Perft.ClearTable();
            Console.WriteLine("ready");
        }
        else if (command.StartsWith("begin"))
        {
            Console.WriteLine("processing");
            var commandParts = command.Split(":");
            if (int.TryParse(commandParts[1], out var depth))
            {
                var (board, whiteToMove) = FenParser.Parse(commandParts[2]);
                var sw = Stopwatch.StartNew();
                var summary = Perft.PerftRoot(ref board, depth, whiteToMove);
                var ms = sw.ElapsedMilliseconds;
                var s = (float)ms / 1000;
                Console.WriteLine($"fen:{board.ToFen(whiteToMove)}");
                Console.WriteLine($"hash:{Zobrist.CalculateZobristKey(ref board, whiteToMove)}");
                Console.WriteLine($"nps:{(summary.Nodes / s)}");
                summary.Print();
                Console.WriteLine("done");
            }
        }
    }
}

return;

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

        commandQueue.Enqueue(command);
        commandAvailable.Set(); // Signal that a command is available
    }
}