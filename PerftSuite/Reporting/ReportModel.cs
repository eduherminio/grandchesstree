namespace PerftSuite.Reporting;

public sealed class Report
{
    public string Tool { get; set; } = "perftcheck";
    public string Version { get; set; } = "0.1.0";
    public string Engine { get; set; } = "";
    public string EngineId { get; set; } = "";
    public DateTime StartedUtc { get; set; }
    public double DurationSeconds { get; set; }
    public RunOptions Options { get; set; } = new();
    public Totals Totals { get; set; } = new();
    public List<FailureEntry> Failures { get; set; } = new();
}

public sealed class RunOptions
{
    public int DepthMin { get; set; }
    public int DepthCap { get; set; }
    public int TimeoutSeconds { get; set; }
    public List<string> EpdFiles { get; set; } = new();
    public string? Filter { get; set; }
    public int? Limit { get; set; }
    public bool FailFast { get; set; }
}

public sealed class Totals
{
    public int Cases   { get; set; }
    public int Passed  { get; set; }
    public int Failed  { get; set; }
    public int Timeout { get; set; }
    public int Error   { get; set; }
}

public sealed class FailureEntry
{
    public string Kind { get; set; } = "";          // "mismatch" | "timeout" | "error"
    public string Fen  { get; set; } = "";
    public int    Depth    { get; set; }
    public ulong? Expected { get; set; }
    public ulong? Actual   { get; set; }
    public long?  Diff     { get; set; }
    public double ElapsedSeconds { get; set; }
    public string Source { get; set; } = "";        // "<file>:<line>"
    public string? Message { get; set; }
    public string? EngineOutput { get; set; }       // truncated
}
