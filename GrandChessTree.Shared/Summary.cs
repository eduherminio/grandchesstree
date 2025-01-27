
using System.Runtime.InteropServices;

namespace GrandChessTree.Client;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Summary
{
    // Grouping 8-byte fields together
    public ulong Nodes;
    public ulong Captures;
    public ulong Enpassant;
    public ulong Castles;
    public ulong Promotions;
    public ulong Checks;
    public ulong DiscoveryChecks;
    public ulong DoubleChecks;
    public ulong CheckMates;
    public ulong FullHash; // Hash should ideally be placed at the end for better alignment

    // Group smaller fields after larger fields
    public int Depth;       // 4 bytes
    public ulong Occupancy; // Aligning 8 bytes after a 4-byte field

    // Padding to prevent false sharing
    private fixed byte _padding[36]; // Ensure struct is 64 bytes for cache line alignment
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
        Console.WriteLine($"nodes:{Nodes}");
        Console.WriteLine($"captures:{Captures}");
        Console.WriteLine($"enpassants:{Enpassant}");
        Console.WriteLine($"castles:{Castles}");
        Console.WriteLine($"promotions:{Promotions}");
        Console.WriteLine($"checks:{Checks}");
        Console.WriteLine($"discovered_checks:{DiscoveryChecks}");
        Console.WriteLine($"double_checks:{DoubleChecks}");
        Console.WriteLine($"check_mates:{CheckMates}");
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

    internal void Update(Summary summary)
    {
        Nodes = summary.Nodes;
        Captures = summary.Captures;
        Enpassant = summary.Enpassant;
        Castles = summary.Castles;
        Promotions = summary.Promotions;
        Checks = summary.Checks;
        DiscoveryChecks = summary.DiscoveryChecks;
        DoubleChecks = summary.DoubleChecks;
        CheckMates = summary.CheckMates;
        FullHash = summary.FullHash;
        Depth = summary.Depth;
        Occupancy = summary.Occupancy;
    }
}