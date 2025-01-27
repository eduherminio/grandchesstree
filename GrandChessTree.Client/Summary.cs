namespace GrandChessTree.Client;

public struct Summary
{
    public ulong Nodes;
    public ulong Captures;
    public ulong Enpassant;
    public ulong Castles;
    public ulong Promotions;
    public ulong Checks;
    public ulong DiscoveryChecks;
    public ulong DoubleChecks;
    public ulong CheckMates;

    public void Accumulate(Summary summary)
    {
        Nodes += summary.Nodes;
        Captures += summary.Captures;
        Enpassant += summary.Enpassant;
        Castles += summary.Castles;
        Promotions += summary.Promotions;
        Checks += summary.Checks;
        DiscoveryChecks += summary.DiscoveryChecks;
        DoubleChecks += summary.DoubleChecks;
        CheckMates += summary.CheckMates;
    }

    public void Print()
    {
        Console.WriteLine($"Nodes: {Nodes}");
        Console.WriteLine($"Captures: {Captures}");
        Console.WriteLine($"Enpassants: {Enpassant}");
        Console.WriteLine($"Castles: {Castles}");
        Console.WriteLine($"Promotions: {Promotions}");
        Console.WriteLine($"Checks: {Checks}");
        Console.WriteLine($"DiscoveryChecks: {DiscoveryChecks}");
        Console.WriteLine($"DoubleChecks: {DoubleChecks}");
        Console.WriteLine($"Check Mates: {CheckMates}");
    }

    internal void AddCapture()
    {
        Captures++;
    }

    internal void AddCastle()
    {
        Castles++;
    }

    internal void AddCheck()
    {
        Checks++;
    }    
    
    internal void AddDoubleCheck()
    {
        Checks++;
        DoubleChecks++;
    }

    internal void AddDiscoveredCheck()
    {
        Checks++;
        DiscoveryChecks++;
    }

    internal void AddMate()
    {
        CheckMates++;
    }

    internal void AddEnpassant()
    {
        Enpassant++;
        Captures++;
    }

    internal void AddPromotion()
    {
        Promotions++;
    }

    internal void AddPromotionCapture()
    {
        Promotions++;
        Captures++;
    }
}