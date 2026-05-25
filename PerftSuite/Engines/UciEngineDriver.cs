using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace PerftSuite.Engines;

/// <summary>
/// Drives a UCI engine over its stdin/stdout pipes. One driver instance owns
/// one engine subprocess; cases are sent serially with sync barriers between
/// them. On timeout the process is killed and the caller is expected to spin
/// up a fresh driver.
/// </summary>
public sealed class UciEngineDriver : IAsyncDisposable
{
    readonly string _enginePath;
    Process?   _process;
    string     _idName = "unknown";

    // Stockfish prints "Nodes searched: N"; other engines sometimes use
    // "Total nodes: N" or just "Total: N". We accept all three.
    static readonly Regex NodesRegex = new(
        @"^\s*(?:nodes(?:\s*searched)?|total(?:\s*nodes)?)\s*:\s*(\d+)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string EngineId => _idName;

    public UciEngineDriver(string enginePath)
    {
        _enginePath = enginePath;
    }

    public async Task StartAsync(int handshakeTimeoutMs, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName               = _enginePath,
            RedirectStandardInput  = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };
        _process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to launch '{_enginePath}'.");

        await SendAsync("uci", ct).ConfigureAwait(false);
        using var hsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        hsCts.CancelAfter(handshakeTimeoutMs);

        string? line;
        while ((line = await ReadLineAsync(hsCts.Token).ConfigureAwait(false)) is not null)
        {
            if (line.StartsWith("id name ", StringComparison.Ordinal))
                _idName = line.Substring("id name ".Length).Trim();
            else if (line.Trim().Equals("uciok", StringComparison.OrdinalIgnoreCase))
                break;
        }
        if (line is null)
            throw new EngineProtocolException("Engine exited before uciok.");

        await SyncAsync(handshakeTimeoutMs, ct).ConfigureAwait(false);
    }

    /// <summary>Send `position fen … / go perft N` and return the total node count.</summary>
    public async Task<PerftRunOutput> RunPerftAsync(
        string fen, int depth, int timeoutMs, CancellationToken ct)
    {
        EnsureRunning();

        var capture = new StringBuilder();
        using var caseCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        caseCts.CancelAfter(timeoutMs);

        await SendAsync($"position fen {fen}", ct).ConfigureAwait(false);
        await SendAsync($"go perft {depth}",   ct).ConfigureAwait(false);

        ulong? nodes = null;
        var sw = Stopwatch.StartNew();
        try
        {
            string? line;
            while ((line = await ReadLineAsync(caseCts.Token).ConfigureAwait(false)) is not null)
            {
                if (capture.Length < 2048)
                {
                    capture.Append(line);
                    capture.Append('\n');
                }

                var m = NodesRegex.Match(line);
                if (m.Success)
                {
                    nodes = ulong.Parse(m.Groups[1].Value);
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (caseCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            sw.Stop();
            return new PerftRunOutput.Timeout(sw.Elapsed, capture.ToString());
        }

        sw.Stop();
        if (nodes is null)
            return new PerftRunOutput.EngineError(
                "Engine closed pipe before producing a node count.", sw.Elapsed, capture.ToString());

        // Sync barrier — make sure the engine is ready for the next position.
        await SyncAsync(timeoutMs, ct).ConfigureAwait(false);
        return new PerftRunOutput.Success(nodes.Value, sw.Elapsed, capture.ToString());
    }

    async Task SyncAsync(int timeoutMs, CancellationToken ct)
    {
        await SendAsync("isready", ct).ConfigureAwait(false);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);
        string? line;
        while ((line = await ReadLineAsync(cts.Token).ConfigureAwait(false)) is not null)
        {
            if (line.Trim().Equals("readyok", StringComparison.OrdinalIgnoreCase))
                return;
        }
        throw new EngineProtocolException("Engine exited before readyok.");
    }

    async Task SendAsync(string command, CancellationToken ct)
    {
        EnsureRunning();
        await _process!.StandardInput.WriteLineAsync(command.AsMemory(), ct).ConfigureAwait(false);
        await _process.StandardInput.FlushAsync(ct).ConfigureAwait(false);
    }

    async Task<string?> ReadLineAsync(CancellationToken ct)
    {
        EnsureRunning();
        return await _process!.StandardOutput.ReadLineAsync(ct).ConfigureAwait(false);
    }

    void EnsureRunning()
    {
        if (_process is null)        throw new InvalidOperationException("Driver not started.");
        if (_process.HasExited)      throw new EngineProtocolException("Engine has exited.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is null) return;
        try
        {
            if (!_process.HasExited)
            {
                try { await SendAsync("quit", CancellationToken.None).ConfigureAwait(false); }
                catch { /* ignore */ }

                if (!_process.WaitForExit(2000))
                    _process.Kill(entireProcessTree: true);
            }
        }
        catch { /* ignore */ }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }
}

public abstract record PerftRunOutput
{
    public sealed record Success(ulong Nodes, TimeSpan Elapsed, string RawOutput) : PerftRunOutput;
    public sealed record Timeout(TimeSpan Elapsed, string RawOutput)               : PerftRunOutput;
    public sealed record EngineError(string Message, TimeSpan Elapsed, string RawOutput) : PerftRunOutput;
}

public sealed class EngineProtocolException : Exception
{
    public EngineProtocolException(string message) : base(message) { }
}
