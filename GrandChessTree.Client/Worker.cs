using System.Diagnostics;

public enum WorkerState
{
    Starting,
    Ready,
    Processing,
    Error,
}

public struct AggregateResultResult
{
    public float Nps;
    public ulong Nodes;
    public ulong Captures;
    public ulong Enpassant;
    public ulong Castles;
    public ulong Promotions;
    public ulong Checks;
    public ulong DiscoveredChecks;
    public ulong DoubleChecks;
    public ulong CheckMates;
    public void Add(WorkerResult result)
    {
        Nps += result.Nps;
        Nodes += result.Nodes;
        Captures += result.Captures;
        Enpassant += result.Enpassant;
        Castles += result.Castles;
        Promotions += result.Promotions;
        Checks += result.Checks;
        DiscoveredChecks += result.DiscoveredChecks;
        DoubleChecks += result.DoubleChecks;
        CheckMates += result.CheckMates;
    }
    
    public void Print()
    {
        Console.WriteLine($"nodes:{Nodes}");
        Console.WriteLine($"captures:{Captures}");
        Console.WriteLine($"enpassants:{Enpassant}");
        Console.WriteLine($"castles:{Castles}");
        Console.WriteLine($"promotions:{Promotions}");
        Console.WriteLine($"checks:{Checks}");
        Console.WriteLine($"discovered_checks:{DiscoveredChecks}");
        Console.WriteLine($"double_checks:{DoubleChecks}");
        Console.WriteLine($"check_mates:{CheckMates}");
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
    public ulong Checks;
    public ulong DiscoveredChecks;
    public ulong DoubleChecks;
    public ulong CheckMates;
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
        result.Checks = outputLogs.GetULongOutputProperty("checks") ?? 0;
        result.DiscoveredChecks = outputLogs.GetULongOutputProperty("discovered_checks") ?? 0;
        result.DoubleChecks = outputLogs.GetULongOutputProperty("double_checks") ?? 0;
        result.CheckMates = outputLogs.GetULongOutputProperty("check_mates") ?? 0;
        result.Hash = outputLogs.GetULongOutputProperty("hash") ?? 0;
        result.Fen = outputLogs.GetStringOutputProperty("fen") ?? "";
        return result;
    }
    private static ulong? GetULongOutputProperty(this List<string> outputLogs, string propertyName)
    {
        var propertyLine = outputLogs.FirstOrDefault(l => l.Contains(propertyName, StringComparison.CurrentCultureIgnoreCase));
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
        if (string.IsNullOrEmpty(args.Data))
        {
            return;
        }

        var loweredOutput = args.Data;

        _outputLogs.Add(loweredOutput);

        if (loweredOutput.Contains("ready"))
        {
            Console.WriteLine($"{WorkerIndex}:ready");
            State = WorkerState.Ready;
        }
        else if (loweredOutput.Contains("done"))
        {
            Console.WriteLine($"{WorkerIndex}:done");
            CollectResults();
            // Process output
            SendCommand("reset");  // Reset or take next task
        }else if (loweredOutput.Contains("processing"))
        {
            Console.WriteLine($"{WorkerIndex}:processing");
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
        SendCommand("quit");
        _process.WaitForExit(); // Block until the process exits
    }

    private void SendCommand(string command)
    {
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

    public void IsReady()
    {
        SendCommand("reset");
    }
}
