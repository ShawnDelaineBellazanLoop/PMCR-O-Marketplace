// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Tools/BeaconScrapeTool.cs
// Identity   : Orchestrator tool — triggers deterministic Beacon scrape loop
// ThoughtLock: 2026-05-31
//
// The orchestrator calls ScrapeVacantProperties when asked by the user.
// The scrape runs in a background Task so the agent response returns immediately.
// The user can ask "scrape status" to poll progress, or just watch
// output\beacon-scrape-progress.json grow.
//
// Three tools are exposed to the orchestrator:
//   ScrapeVacantProperties — start (or resume) the scrape
//   GetScrapeStatus        — poll current state of the background job
//   CancelScrape           — cancel with progress preserved
// ═══════════════════════════════════════════════════════════════════════════════

using System.ComponentModel;
using ProjectName.AgentService.Services;

namespace ProjectName.AgentService.Tools;

public static class BeaconScrapeTool
{
    private static Task<string>?           _runningJob;
    private static CancellationTokenSource? _cts;
    private static string                   _lastStatus = "Not started";
    private static readonly object          _lock       = new();

    [Description(
        "Start (or resume) scraping all 402 vacant properties from Beacon (Ramsey County). " +
        "Reads addresses from csvPath, scrapes each one via the Playwright MCP, and saves results to " +
        "outputDir\\beacon-results.json. Progress is crash-safe — restarts pick up where they left off. " +
        "Returns immediately; scraping runs in the background. " +
        "Check status with GetScrapeStatus or by reading outputDir\\beacon-scrape-progress.json.")]
    public static string ScrapeVacantProperties(
        [Description("Absolute path to vacant_properties.csv")] string csvPath,
        [Description("Absolute path to output directory (e.g. A:\\PMCR-O\\output)")] string outputDir,
        IServiceProvider services)
    {
        lock (_lock)
        {
            if (_runningJob is { IsCompleted: false })
                return "Beacon scrape already running. Use GetScrapeStatus to check progress.";

            _cts      = new CancellationTokenSource();
            var token = _cts.Token;
            var svc   = services.GetRequiredService<BeaconScrapeService>();

            _lastStatus = "Starting...";
            _runningJob = Task.Run(async () =>
            {
                try
                {
                    _lastStatus = "Running...";
                    var result  = await svc.RunAsync(csvPath, outputDir, token);
                    _lastStatus = result;
                    return result;
                }
                catch (OperationCanceledException)
                {
                    _lastStatus = "Cancelled.";
                    return "Cancelled.";
                }
                catch (Exception ex)
                {
                    _lastStatus = $"ERROR: {ex.Message}";
                    return _lastStatus;
                }
            }, token);

            return $"Beacon scrape started in background. " +
                   $"CSV: {csvPath} → {outputDir}\\beacon-results.json. " +
                   "Use GetScrapeStatus to poll, or read beacon-scrape-progress.json directly.";
        }
    }

    [Description("Return the current status of the Beacon property scrape background job.")]
    public static string GetScrapeStatus()
    {
        lock (_lock)
        {
            if (_runningJob is null)                 return $"No job running. Last: {_lastStatus}";
            if (_runningJob.IsCompletedSuccessfully) return $"Completed: {_lastStatus}";
            if (_runningJob.IsFaulted)               return $"Faulted: {_runningJob.Exception?.GetBaseException().Message}";
            if (_runningJob.IsCanceled)              return "Cancelled.";
            return $"Running. Last update: {_lastStatus}";
        }
    }

    [Description("Cancel a running Beacon scrape. Progress already written to JSON is preserved.")]
    public static string CancelScrape()
    {
        lock (_lock)
        {
            if (_cts is null || (_runningJob?.IsCompleted ?? true))
                return "No active scrape to cancel.";
            _cts.Cancel();
            return "Cancel requested. Progress saved so far is preserved.";
        }
    }
}
