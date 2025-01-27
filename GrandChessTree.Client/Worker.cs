using System.Diagnostics;

public enum WorkerState
{
    Starting,
    Ready,
    Processing,
    Error,
}

public struct WorkerResult
{
    public float Nps { get; set; }
}

public class Worker
{
    private readonly Process _process;
    private readonly List<string> _outputLogs;
    private readonly List<string> _errorLogs;
    public Queue<WorkerResult> WorkerResults { get; private set; } = new Queue<WorkerResult>();
    public WorkerState State { get; private set; } = WorkerState.Starting;

    public Worker(string workerPath, string arguments, int workerIndex)
    {
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

        var loweredOutput = args.Data.ToLower();
        Console.WriteLine($"<={loweredOutput}");

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
        }
    }

    public void CollectResults()
    {
        WorkerResult summary = default;
        var workerNpsLog = _outputLogs.FirstOrDefault(l => l.ToLower().Contains("nps"));
        if (!string.IsNullOrEmpty(workerNpsLog))
        {
            var workerNpsParts = workerNpsLog.Split(":");
            if (workerNpsParts.Length == 2 && float.TryParse(workerNpsParts[1], out var workerNps))
            {
                summary.Nps = workerNps;
            }
        }

        _outputLogs.Clear();
    }

    private void OnErrorReceived(DataReceivedEventArgs args)
    {
        if (args.Data != null)
        {
            _errorLogs.Add($"ERROR: {args.Data}"); // Capture errors
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
        _process.WaitForExit(); // Block until the process exits
    }

    private void SendCommand(string command)
    {
        // Write a command to the worker's standard input
        if (!_process.HasExited)
        {
            Console.WriteLine($"=>{command}");
            _process.StandardInput.WriteLine(command);
        }
    }

    public List<string> GetOutputLogs() => _outputLogs;
    public List<string> GetErrorLogs() => _errorLogs;

    public void NextTask(string nextFen)
    {
        SendCommand(nextFen);
    }

    public void IsReady()
    {
        SendCommand("reset");
    }
}
