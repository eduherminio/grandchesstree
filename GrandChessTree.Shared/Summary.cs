
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GrandChessTree.Shared;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
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
    public ulong FullHash;
    public byte Depth;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Accumulate(ref Summary summary)
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddCapture()
    {
        Captures++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddCastle()
    {
        Castles++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddCheck()
    {
        Checks++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddDoubleCheck()
    {
        Checks++;
        DoubleChecks++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddDiscoveredCheck()
    {
        Checks++;
        DiscoveryChecks++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddDoubleDiscoveredCheck()
    {
        Checks++;
        DoubleChecks++;
        DiscoveryChecks++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddMate()
    {
        Checks++;
        CheckMates++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddEnpassant()
    {
        Enpassant++;
        Captures++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddPromotion()
    {
        Promotions+=4;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    internal void AddPromotionCapture()
    {
        Promotions+=4;
        Captures+=4;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

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
    }
}