using System.Diagnostics;
using System.Runtime.CompilerServices;
using GrandChessTree.Client;
using GrandChessTree.Client.Tables;

Console.WriteLine("-----TheGreatChessTree-----");

var board = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

RuntimeHelpers.RunClassConstructor(typeof(AttackTables).TypeHandle);

var boards = MoveGenerator.PerftRoot(ref board, 2, true);
Console.WriteLine($"Split into {boards.Length} tasks");

Summary totalSummery = default;
var locker = new object();

var sw = Stopwatch.StartNew();

Parallel.For(0, boards.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
{
    Summary summary = default;
    var innerBoard = boards[i];
    Perft.PerftRoot(ref innerBoard, ref summary, 7, true);

    lock (locker)
    {
        totalSummery.Accumulate(summary);
    }
});

var ms = sw.ElapsedMilliseconds;
var s = (float)ms / 1000;
Console.WriteLine($"took {s}s");
Console.WriteLine($"{(totalSummery.Nodes / s).FormatBigNumber()}nps");
totalSummery.Print();