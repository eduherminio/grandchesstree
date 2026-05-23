using PerftSuite.Engines;
using PerftSuite.Epd;

namespace PerftSuite.Runner;

public sealed class PerftRunner
{
    public required string EnginePath { get; init; }
    public required int    TimeoutSeconds { get; init; }
    public required bool   FailFast { get; init; }

    UciEngineDriver? _driver;
    string _engineId = "unknown";

    public string EngineId => _engineId;

    public async Task<IReadOnlyList<CaseResult>> RunAsync(
        IReadOnlyList<EpdCase> cases,
        Action<CaseResult>     onCaseComplete,
        CancellationToken      ct)
    {
        await EnsureDriverAsync(ct).ConfigureAwait(false);
        var results = new List<CaseResult>(cases.Count);
        int timeoutMs = TimeoutSeconds * 1000;

        foreach (var c in cases)
        {
            if (ct.IsCancellationRequested) break;

            CaseResult r;
            try
            {
                var output = await _driver!.RunPerftAsync(c.Fen, c.Depth, timeoutMs, ct)
                    .ConfigureAwait(false);

                r = output switch
                {
                    PerftRunOutput.Success s when s.Nodes == c.Expected =>
                        new CaseResult(c, CaseStatus.Pass, s.Nodes, s.Elapsed.TotalSeconds, null, null),

                    PerftRunOutput.Success s =>
                        new CaseResult(c, CaseStatus.Mismatch, s.Nodes, s.Elapsed.TotalSeconds, null, null),

                    PerftRunOutput.Timeout t =>
                        new CaseResult(c, CaseStatus.Timeout, null, t.Elapsed.TotalSeconds, "case timed out", Trunc(t.RawOutput)),

                    PerftRunOutput.EngineError e =>
                        new CaseResult(c, CaseStatus.EngineError, null, e.Elapsed.TotalSeconds, e.Message, Trunc(e.RawOutput)),

                    _ => throw new InvalidOperationException("unreachable"),
                };
            }
            catch (EngineProtocolException ex)
            {
                r = new CaseResult(c, CaseStatus.EngineError, null, 0, ex.Message, null);
            }

            // Replace the engine if it crashed / timed out — the contract
            // of the driver is that it spins up fresh after a timeout.
            if (r.Status is CaseStatus.Timeout or CaseStatus.EngineError)
            {
                await DisposeDriverAsync().ConfigureAwait(false);
                await EnsureDriverAsync(ct).ConfigureAwait(false);
            }

            results.Add(r);
            onCaseComplete(r);

            if (FailFast && r.Status != CaseStatus.Pass) break;
        }

        await DisposeDriverAsync().ConfigureAwait(false);
        return results;
    }

    async Task EnsureDriverAsync(CancellationToken ct)
    {
        if (_driver is not null) return;
        var d = new UciEngineDriver(EnginePath);
        await d.StartAsync(handshakeTimeoutMs: 10_000, ct).ConfigureAwait(false);
        _driver = d;
        if (_engineId == "unknown")
            _engineId = d.EngineId;
    }

    async Task DisposeDriverAsync()
    {
        if (_driver is null) return;
        await _driver.DisposeAsync().ConfigureAwait(false);
        _driver = null;
    }

    static string Trunc(string s, int max = 512)
        => s.Length <= max ? s : s.Substring(0, max) + "…";
}
