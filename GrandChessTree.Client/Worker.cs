using System.Diagnostics;
using GrandChessTree.Shared;

namespace GrandChessTree.Client;

public enum WorkerState
{
    Starting,
    Ready,
    Processing,
    Error,
    Exited,
}

public class AggregateResultResult
{
    public float Nps;
    public ulong Nodes;
    public ulong Captures;
    public ulong Enpassant;
    public ulong Castles;
    public ulong Promotions;
    public ulong DirectCheck;
    public ulong SingleDiscoveredCheck;
    public ulong DirectDiscoveredCheck;
    public ulong DoubleDiscoveredCheck;
    public ulong DirectCheckmate;
    public ulong SingleDiscoveredCheckmate;
    public ulong DirectDiscoverdCheckmate;
    public ulong DoubleDiscoverdCheckmate;
    public void Add(WorkerResult result)
    {
        Nps += result.Nps;
        Nodes += result.Nodes;
        Captures += result.Captures;
        Enpassant += result.Enpassant;
        Castles += result.Castles;
        Promotions += result.Promotions;
        DirectCheck += result.DirectCheck;
        SingleDiscoveredCheck += result.SingleDiscoveredCheck;
        DirectDiscoveredCheck += result.DirectDiscoveredCheck;
        DoubleDiscoveredCheck += result.DoubleDiscoveredCheck;
        DirectCheckmate += result.DirectCheckmate;
        SingleDiscoveredCheckmate += result.SingleDiscoveredCheckmate;
        DirectDiscoverdCheckmate += result.DirectDiscoverdCheckmate;
        DoubleDiscoverdCheckmate += result.DoubleDiscoverdCheckmate;
    }
    
    public void Print()
    {
        Console.WriteLine($"nodes:{Nodes}");
        Console.WriteLine($"captures:{Captures}");
        Console.WriteLine($"enpassants:{Enpassant}");
        Console.WriteLine($"castles:{Castles}");
        Console.WriteLine($"promotions:{Promotions}");
        Console.WriteLine($"direct_checks:{DirectCheck}");
        Console.WriteLine($"single_discovered_checks:{SingleDiscoveredCheck}");
        Console.WriteLine($"direct_discovered_checks:{DirectDiscoveredCheck}");
        Console.WriteLine($"double_discovered_check:{DoubleDiscoveredCheck}");
        Console.WriteLine($"total_checks:{DirectCheck + SingleDiscoveredCheck + DirectDiscoveredCheck + DoubleDiscoveredCheck}");

        Console.WriteLine($"direct_mates:{DirectCheckmate}");
        Console.WriteLine($"single_discovered_mates:{SingleDiscoveredCheckmate}");
        Console.WriteLine($"direct_discoverd_mates:{DirectDiscoverdCheckmate}");
        Console.WriteLine($"double_discoverd_mates:{DoubleDiscoverdCheckmate}");
        Console.WriteLine($"total_mates:{DirectCheckmate + SingleDiscoveredCheckmate + DirectDiscoverdCheckmate + DoubleDiscoverdCheckmate}");
    }
}

public struct WorkerResult
{
    public float Nps;
    public ulong Nodes;
    public ulong Captures;
    public ulong Enpassant;
    public ulong Castles;
    public ulong Promotions;
    public ulong DirectCheck;
    public ulong SingleDiscoveredCheck;
    public ulong DirectDiscoveredCheck;
    public ulong DoubleDiscoveredCheck;
    public ulong DirectCheckmate;
    public ulong SingleDiscoveredCheckmate;
    public ulong DirectDiscoverdCheckmate;
    public ulong DoubleDiscoverdCheckmate;
    public string Fen;
    public ulong Hash;
}

public static class WorkerResultExtensions
{
    public static WorkerResult ParseWorkerResult(this List<string> outputLogs)
    {
        WorkerResult result = default;
        result.Nps = outputLogs.GetFloatOutputProperty("nps") ?? 0;
        result.Nodes = outputLogs.GetULongOutputProperty("nodes") ?? 0;
        result.Captures = outputLogs.GetULongOutputProperty("captures") ?? 0;
        result.Enpassant = outputLogs.GetULongOutputProperty("enpassants") ?? 0;
        result.Castles = outputLogs.GetULongOutputProperty("castles") ?? 0;
        result.Promotions = outputLogs.GetULongOutputProperty("promotions") ?? 0;
        result.DirectCheck = outputLogs.GetULongOutputProperty("direct_checks") ?? 0;
        result.SingleDiscoveredCheck = outputLogs.GetULongOutputProperty("single_discovered_checks") ?? 0;
        result.DirectDiscoveredCheck = outputLogs.GetULongOutputProperty("direct_discovered_checks") ?? 0;
        result.DoubleDiscoveredCheck = outputLogs.GetULongOutputProperty("double_discovered_check") ?? 0;
        result.DirectCheckmate = outputLogs.GetULongOutputProperty("direct_mates") ?? 0;
        result.SingleDiscoveredCheckmate = outputLogs.GetULongOutputProperty("single_discovered_mates") ?? 0;
        result.DirectDiscoverdCheckmate = outputLogs.GetULongOutputProperty("direct_discoverd_mates") ?? 0;
        result.DoubleDiscoverdCheckmate = outputLogs.GetULongOutputProperty("double_discoverd_mates") ?? 0;
        result.Hash = outputLogs.GetULongOutputProperty("hash") ?? 0;
        result.Fen = outputLogs.GetStringOutputProperty("fen") ?? "";
        return result;
    }
    private static ulong? GetULongOutputProperty(this List<string> outputLogs, string propertyName)
    {
        var propertyLine = outputLogs.FirstOrDefault(l => l.StartsWith(propertyName, StringComparison.CurrentCultureIgnoreCase));
        if (string.IsNullOrEmpty(propertyLine)) return null;
        
        var lineParts = propertyLine.Split(":");
        if (lineParts.Length == 2 && ulong.TryParse(lineParts[1], out var value))
        {
            return value;
        }

        return null;
    }
    private static string? GetStringOutputProperty(this List<string> outputLogs, string propertyName)
    {
        var propertyLine = outputLogs.FirstOrDefault(l => l.Contains(propertyName, StringComparison.CurrentCultureIgnoreCase));
        if (string.IsNullOrEmpty(propertyLine)) return null;
        
        var lineParts = propertyLine.Split(":");
        if (lineParts.Length == 2)
        {
            return lineParts[1];
        }

        return null;
    }
    
    private static float? GetFloatOutputProperty(this List<string> outputLogs, string propertyName)
    {
        var propertyLine = outputLogs.FirstOrDefault(l => l.Contains(propertyName, StringComparison.CurrentCultureIgnoreCase));
        if (string.IsNullOrEmpty(propertyLine)) return null;
        
        var lineParts = propertyLine.Split(":");
        if (lineParts.Length == 2 && float.TryParse(lineParts[1], out var value))
        {
            return value;
        }

        return null;
    }
}

public class Worker
{
    private readonly Process _process;
    private readonly List<string> _outputLogs;
    private readonly List<string> _errorLogs;
    public Queue<WorkerResult> WorkerResults { get; private set; } = new Queue<WorkerResult>();
    public WorkerState State { get; private set; } = WorkerState.Starting;
    public int WorkerIndex { get; private set; } = 0;
    public Worker(string workerPath, string arguments, int workerIndex)
    {
        WorkerIndex = workerIndex;
        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = workerPath,
                Arguments = arguments,
                RedirectStandardOutput = true,  // This allows reading the output
                RedirectStandardError = true,   // This allows reading the errors
                RedirectStandardInput = true,   // This is needed to write to standard input
                UseShellExecute = false,  // Required for redirection
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
            }
        };

        _outputLogs = new List<string>();
        _errorLogs = new List<string>();

        _process.OutputDataReceived += (sender, args) => OnOutputReceived(args);
        _process.ErrorDataReceived += (sender, args) => OnErrorReceived(args);
    }

    private void OnOutputReceived(DataReceivedEventArgs args)
    {
        if(State == WorkerState.Exited)
        {
            return;
        }

        if (string.IsNullOrEmpty(args.Data))
        {
            return;
        }

        var loweredOutput = args.Data;

        _outputLogs.Add(loweredOutput);

        if (loweredOutput.Contains("ready"))
        {
            State = WorkerState.Ready;
        }
        else if (loweredOutput.Contains("done"))
        {
            CollectResults();
            // Process output
            SendCommand("reset");  // Reset or take next task
        }else if (loweredOutput.Contains("processing"))
        {
            State = WorkerState.Processing;
        }
    }

    private void CollectResults()
    {
        var summary = _outputLogs.ParseWorkerResult();
        WorkerResults.Enqueue(summary);
        _outputLogs.Clear();
    }

    private void OnErrorReceived(DataReceivedEventArgs args)
    {
        if (args.Data != null)
        {
            Console.WriteLine($"ERROR: {args.Data}");
        }
    }

    public void Start()
    {
        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    public void WaitForExit()
    {
        if (State == WorkerState.Exited)
        {
            return;
        }

        if (_process.HasExited)
        {
            return;
        }
        State = WorkerState.Exited;

        SendCommand("quit");
        _process.WaitForExit(); // Block until the process exits
    }

    private void SendCommand(string command)
    {
        if (State == WorkerState.Exited)
        {
            return;
        }

        // Write a command to the worker's standard input
        if (!_process.HasExited)
        {
            _process.StandardInput.WriteLine(command);
        }
    }

    public List<string> GetOutputLogs() => _outputLogs;
    public List<string> GetErrorLogs() => _errorLogs;

    public void NextTask(string nextFen)
    {
        SendCommand(nextFen);
        State = WorkerState.Processing;
    }

    public void Reset()
    {
        SendCommand("reset");
    }

    internal void Kill()
    {
        if (State == WorkerState.Exited)
        {
            return;
        }
        State = WorkerState.Exited;

        if (!_process.HasExited)
        {
            SendCommand("quit");
            _process.Kill();
            _process.WaitForExit();
            _process.Dispose();
        }
    }
}