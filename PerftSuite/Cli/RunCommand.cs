using System.ComponentModel;
using System.Diagnostics;
using PerftSuite.Epd;
using PerftSuite.Reporting;
using PerftSuite.Runner;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PerftSuite.Cli;

public sealed class RunCommand : AsyncCommand<RunCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-e|--engine <PATH>")]
        [Description("Path to the UCI engine executable.")]
        public required string Engine { get; init; }

        [CommandOption("--epd <FILE>")]
        [Description("EPD file(s) to use instead of the bundled corpora. Repeatable.")]
        public string[]? Epd { get; init; }

        [CommandOption("--depth-cap <N>")]
        [Description("Maximum perft depth to test.")]
        [DefaultValue(4)]
        public int DepthCap { get; init; }

        [CommandOption("--depth-min <N>")]
        [Description("Minimum perft depth to test.")]
        [DefaultValue(1)]
        public int DepthMin { get; init; }

        [CommandOption("--timeout <SECS>")]
        [Description("Per-case timeout in seconds.")]
        [DefaultValue(30)]
        public int Timeout { get; init; }

        [CommandOption("--filter <SUBSTR>")]
        [Description("Only run cases whose FEN contains this substring.")]
        public string? Filter { get; init; }

        [CommandOption("--limit <N>")]
        [Description("Test only the first N matching cases.")]
        public int? Limit { get; init; }

        [CommandOption("--report <PATH>")]
        [Description("Path to write the JSON report.")]
        [DefaultValue("perft-report.json")]
        public string Report { get; init; } = "perft-report.json";

        [CommandOption("--fail-fast")]
        [Description("Stop on the first failure.")]
        public bool FailFast { get; init; }

        [CommandOption("--quiet")]
        [Description("Suppress per-case console output.")]
        public bool Quiet { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext ctx, Settings s, CancellationToken cancellationToken)
    {
        if (!File.Exists(s.Engine))
        {
            AnsiConsole.MarkupLine($"[red]Engine not found:[/] {s.Engine}");
            return 2;
        }

        if (s.DepthMin < 1 || s.DepthCap < s.DepthMin)
        {
            AnsiConsole.MarkupLine("[red]Invalid depth range.[/]");
            return 2;
        }

        // Load corpora
        List<EpdCase> cases;
        List<string>  sources;
        try
        {
            (cases, sources) = LoadCases(s);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]EPD load failed:[/] {ex.Message}");
            return 2;
        }

        if (cases.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No cases selected.[/]");
            return 2;
        }

        var runner = new PerftRunner
        {
            EnginePath     = s.Engine,
            TimeoutSeconds = s.Timeout,
            FailFast       = s.FailFast,
        };

        // Header table
        if (!s.Quiet)
        {
            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(2))
                .AddColumn();
            grid.AddRow("[grey]engine[/]",  s.Engine);
            grid.AddRow("[grey]corpora[/]", string.Join(", ", sources));
            grid.AddRow("[grey]depths[/]",  $"{s.DepthMin}–{s.DepthCap}");
            grid.AddRow("[grey]cases[/]",   cases.Count.ToString());
            grid.AddRow("[grey]timeout[/]", $"{s.Timeout}s");
            grid.AddRow("[grey]report[/]",  s.Report);
            AnsiConsole.Write(new Panel(grid).Header(" perftcheck ").Border(BoxBorder.Rounded));
        }

        var stopwatch = Stopwatch.StartNew();
        var ctsLifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; ctsLifetime.Cancel(); };

        IReadOnlyList<CaseResult> results;
        if (s.Quiet)
        {
            results = await runner.RunAsync(cases, _ => {}, ctsLifetime.Token);
        }
        else
        {
            results = await AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn())
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("running", maxValue: cases.Count);
                    int pass = 0, fail = 0;
                    return await runner.RunAsync(cases, r =>
                    {
                        if (r.Status == CaseStatus.Pass) pass++;
                        else                              fail++;
                        task.Description =
                            $"[green]{pass} pass[/]  [red]{fail} fail[/]";
                        task.Increment(1);
                    }, ctsLifetime.Token);
                });
        }

        stopwatch.Stop();

        // Build report
        var totals = new Totals();
        var failures = new List<FailureEntry>();
        foreach (var r in results)
        {
            totals.Cases++;
            switch (r.Status)
            {
                case CaseStatus.Pass:        totals.Passed++; break;
                case CaseStatus.Mismatch:    totals.Failed++; failures.Add(ReportWriter.MakeFailure(r)); break;
                case CaseStatus.Timeout:     totals.Timeout++; failures.Add(ReportWriter.MakeFailure(r)); break;
                case CaseStatus.EngineError: totals.Error++;   failures.Add(ReportWriter.MakeFailure(r)); break;
            }
        }

        var report = new Report
        {
            Engine          = Path.GetFullPath(s.Engine),
            EngineId        = runner.EngineId,
            StartedUtc      = DateTime.UtcNow.Subtract(stopwatch.Elapsed),
            DurationSeconds = stopwatch.Elapsed.TotalSeconds,
            Options = new RunOptions
            {
                DepthMin       = s.DepthMin,
                DepthCap       = s.DepthCap,
                TimeoutSeconds = s.Timeout,
                EpdFiles       = sources,
                Filter         = s.Filter,
                Limit          = s.Limit,
                FailFast       = s.FailFast,
            },
            Totals   = totals,
            Failures = failures,
        };

        try
        {
            ReportWriter.Write(s.Report, report);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to write report '{s.Report}':[/] {ex.Message}");
            return 1;
        }

        if (!s.Quiet)
        {
            PrintSummary(report);
            PrintReportLocation(s.Report);
        }

        int badCount = totals.Failed + totals.Timeout + totals.Error;
        return badCount == 0 ? 0 : 1;
    }

    static (List<EpdCase>, List<string>) LoadCases(Settings s)
    {
        var raw = new List<EpdCase>();
        var sources = new List<string>();

        if (s.Epd is { Length: > 0 })
        {
            foreach (var path in s.Epd)
            {
                sources.Add(Path.GetFileName(path));
                raw.AddRange(EpdReader.ReadFile(path));
            }
        }
        else
        {
            foreach (var name in EpdReader.BundledNames)
            {
                sources.Add(name);
                raw.AddRange(EpdReader.ReadBundled(name));
            }
        }

        IEnumerable<EpdCase> q = raw
            .Where(c => c.Depth >= s.DepthMin && c.Depth <= s.DepthCap);
        if (!string.IsNullOrEmpty(s.Filter))
            q = q.Where(c => c.Fen.Contains(s.Filter, StringComparison.Ordinal));
        if (s.Limit is int n)
            q = q.Take(n);

        return (q.ToList(), sources);
    }

    static void PrintSummary(Report r)
    {
        var t = new Table().Border(TableBorder.Rounded).AddColumn("metric").AddColumn(new TableColumn("value").RightAligned());
        t.AddRow("engine",   Markup.Escape(r.EngineId));
        t.AddRow("duration", $"{r.DurationSeconds:F2}s");
        t.AddRow("cases",    r.Totals.Cases.ToString());
        t.AddRow("[green]passed[/]",  r.Totals.Passed.ToString());
        t.AddRow(r.Totals.Failed  > 0 ? "[red]failed[/]"  : "failed",  r.Totals.Failed.ToString());
        t.AddRow(r.Totals.Timeout > 0 ? "[red]timeout[/]" : "timeout", r.Totals.Timeout.ToString());
        t.AddRow(r.Totals.Error   > 0 ? "[red]error[/]"   : "error",   r.Totals.Error.ToString());
        AnsiConsole.Write(t);

        if (r.Failures.Count > 0)
        {
            int show = Math.Min(r.Failures.Count, 10);
            AnsiConsole.MarkupLine($"\n[red]Showing {show} of {r.Failures.Count} failure(s):[/]");
            foreach (var f in r.Failures.Take(show))
            {
                var lines = new List<string>
                {
                    $"[grey]source[/] {Markup.Escape(f.Source)}",
                    $"[grey]fen[/]    {Markup.Escape(f.Fen)}",
                    $"[grey]depth[/]  {f.Depth}   [grey]kind[/] {f.Kind}",
                };
                if (f.Kind == "mismatch")
                    lines.Add($"[red]expected[/] {f.Expected:N0}   [red]actual[/] {f.Actual:N0}   [red]diff[/] {f.Diff}");
                if (!string.IsNullOrEmpty(f.Message))
                    lines.Add($"[red]message[/] {Markup.Escape(f.Message)}");
                AnsiConsole.Write(new Panel(string.Join("\n", lines)).Border(BoxBorder.Rounded));
            }
        }

    }

    static void PrintReportLocation(string reportPath)
    {
        AnsiConsole.MarkupLine($"\nReport written to [blue]{Markup.Escape(Path.GetFullPath(reportPath))}[/]");
    }
}
