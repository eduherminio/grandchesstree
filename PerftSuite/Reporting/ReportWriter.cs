using System.Text.Json;
using PerftSuite.Runner;

namespace PerftSuite.Reporting;

public static class ReportWriter
{
    static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public static void Write(string path, Report report)
    {
        string json = JsonSerializer.Serialize(report, Options);
        File.WriteAllText(path, json);
    }

    public static FailureEntry MakeFailure(CaseResult r)
    {
        var fe = new FailureEntry
        {
            Fen      = r.Case.Fen,
            Depth    = r.Case.Depth,
            Expected = r.Case.Expected,
            Actual   = r.Actual,
            ElapsedSeconds = r.ElapsedSeconds,
            Source   = $"{r.Case.SourceFile}:{r.Case.SourceLine}",
            Message  = r.ErrorMessage,
            EngineOutput = r.RawEngineOutput,
        };
        fe.Kind = r.Status switch
        {
            CaseStatus.Mismatch    => "mismatch",
            CaseStatus.Timeout     => "timeout",
            CaseStatus.EngineError => "error",
            _                      => "unknown",
        };
        if (r.Status == CaseStatus.Mismatch && r.Actual.HasValue)
            fe.Diff = (long)r.Actual.Value - (long)r.Case.Expected;
        return fe;
    }
}
