using ConsoleTables;
using GrandChessTree.Client;
using GrandChessTree.Shared.Helpers;

public static class OutputHelpers
{
    public static void PrintReport(string fen, int depth, AggregateResultResult result)
    {
        
        var table = new ConsoleTable("key", "value");
        table
            .AddRow("fen", fen)
            .AddRow("depth", depth)
            .AddRow("nps", result.Nps.FormatBigNumber())
            .AddRow("nodes", result.Nodes.FormatBigNumber())
            .AddRow("captures", result.Captures.FormatBigNumber())
            .AddRow("enpassants", result.Enpassant.FormatBigNumber())
            .AddRow("castles", result.Castles.FormatBigNumber())
            .AddRow("promotions", result.Promotions.FormatBigNumber())
            .AddRow("direct_checks", result.DirectCheck.FormatBigNumber())
            .AddRow("single_discovered_checks", result.SingleDiscoveredCheck.FormatBigNumber())
            .AddRow("direct_discovered_checks", result.DirectDiscoveredCheck.FormatBigNumber())
            .AddRow("double_discovered_check", result.DoubleDiscoveredCheck.FormatBigNumber())
            .AddRow("total_checks", (result.DirectCheck + result.SingleDiscoveredCheck + result.DirectDiscoveredCheck + result.DoubleDiscoveredCheck).FormatBigNumber())
            .AddRow("direct_mates", result.DirectCheckmate.FormatBigNumber())
            .AddRow("single_discovered_mates", result.SingleDiscoveredCheckmate.FormatBigNumber())
            .AddRow("direct_discoverd_mates", result.DirectDiscoverdCheckmate.FormatBigNumber())
            .AddRow("double_discoverd_mates", result.DoubleDiscoverdCheckmate.FormatBigNumber())
            .AddRow("total_mates", (result.DirectCheckmate + result.SingleDiscoveredCheckmate + result.DirectDiscoverdCheckmate + result.DoubleDiscoverdCheckmate).FormatBigNumber());

        table.Configure((c) =>
        {
            c.EnableCount = false;
        });

        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);
        for (int y = 0; y < Console.WindowHeight; y++)
            Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, 0);

        table.Write(Format.MarkDown);
    }


}