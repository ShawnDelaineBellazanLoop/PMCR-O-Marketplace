// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Services/BeaconScrapeService.cs
// Identity   : Deterministic Beacon property scraper — loops 402 vacant properties
// Law Anchor : EC-004 (no fan-out), SEQUENTIAL-001 (serial browser ops)
// ThoughtLock: 2026-05-31
//
// ARCHITECTURE NOTE — Why McpToolCache, not raw HttpClient:
//   The Playwright MCP is a stdio server (WithStdioServerTransport). It has no
//   HTTP endpoint. BeaconScrapeService must call Playwright via the AIFunction
//   delegates already registered in McpToolCache — those are wired correctly by
//   MAF's MCP infrastructure. Calling http://projectname-mcp-playwright directly
//   will always fail with connection refused.
//
//   Filesystem MCP IS an HTTP server, but for consistency and to avoid duplicate
//   wiring, we also go through McpToolCache for file I/O.
//
// DESIGN — Why not a MAF Workflow?
//   MAF Workflows are designed for multi-agent, multi-turn AI coordination.
//   This task is fully deterministic: read CSV → loop → scrape → write JSON.
//   A plain service invoking AIFunction delegates directly does not burn model
//   tokens on loop control and is crash-safe via the progress file pattern.
//
// PROGRESS TRACKING
//   beacon-scrape-progress.json written after every property.
//   On restart the scraper reads this and skips already-completed addresses.
//   A crash mid-property loses that one property only — not the whole run.
//
// BEACON SCRAPE SEQUENCE (per property)
//   1. Navigate to Beacon search page
//   2. Dismiss T&C if present (once per navigate)
//   3. Fill address input
//   4. Click Search, wait for results
//   5. Click first parcel link
//   6. playwright.evaluate to extract: PIN, Owner, Market Value, Delinquent Taxes, Forfeiture Year
//   7. Append to beacon-results.json
//   8. Sleep 1500ms (polite rate limit)
//
// OUTPUT FILES (under outputDir\)
//   beacon-results.json          — array of PropertyResult
//   beacon-scrape-progress.json  — { completed: string[], failed: [{address,error}] }
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectName.AgentService.Services;

// ── Result / progress model types ────────────────────────────────────────────

public sealed record PropertyResult(
    string Address,
    string Pin,
    string Owner,
    string MarketValue,
    string DelinquentTaxes,
    string ForfeitureYear,
    string ScrapedAt);

public sealed class ScrapeProgress
{
    public List<string>         Completed { get; set; } = [];
    public List<FailedProperty> Failed    { get; set; } = [];
}

public sealed record FailedProperty(string Address, string Error);

// ── LoggerMessage delegates (CA1848 / CA1873) ─────────────────────────────────

internal static partial class BeaconLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "[Beacon] Loaded {Count} addresses from {Csv}")]
    internal static partial void Loaded(ILogger logger, int count, string csv);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Beacon] Progress: {Done} done, {Remaining} remaining")]
    internal static partial void Progress(ILogger logger, int done, int remaining);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Beacon] [{N}/{Total}] {Address}")]
    internal static partial void Scraping(ILogger logger, int n, int total, string address);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Beacon] OK  {Address} | {Owner} | {Pin}")]
    internal static partial void ScrapedOk(ILogger logger, string address, string owner, string pin);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Beacon] ERR {Address}: {Error}")]
    internal static partial void ScrapedErr(ILogger logger, string address, string error);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[Beacon] Parse failed {Address}: {Err}")]
    internal static partial void ParseFailed(ILogger logger, string address, string err);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Beacon] Tool '{Tool}' not found in McpToolCache — scrape will fail")]
    internal static partial void ToolMissing(ILogger logger, string tool);
}

// ── Tool caller ───────────────────────────────────────────────────────────────

/// <summary>
/// Thin wrapper that resolves named AIFunction delegates from McpToolCache
/// and invokes them with a simple string-keyed argument dictionary.
/// This is how BeaconScrapeService calls Playwright and Filesystem without
/// going through the AI agent loop or raw HTTP.
/// </summary>
internal sealed class McpCaller(McpToolCache cache, ILogger logger)
{
    // Build a lookup once — tool names are stable after startup
    private readonly Dictionary<string, AIFunction> _tools = cache
        .GetNativeTools()
        .OfType<AIFunction>()
        .ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

    public async Task<string> CallAsync(
        string toolName,
        Dictionary<string, object?> args,
        CancellationToken ct)
    {
        if (!_tools.TryGetValue(toolName, out var fn))
        {
            BeaconLog.ToolMissing(logger, toolName);
            return $"ERROR: tool '{toolName}' not registered";
        }

        var arguments = new AIFunctionArguments(args);
        var result    = await fn.InvokeAsync(arguments, ct);
        return result?.ToString() ?? string.Empty;
    }
}

// ── Service ───────────────────────────────────────────────────────────────────

/// <summary>
/// Deterministic Beacon scraper. Inject via DI and call RunAsync() from BeaconScrapeTool.
/// Calls Playwright and Filesystem via McpToolCache AIFunction delegates — no HTTP, no AI loop.
/// </summary>
public sealed class BeaconScrapeService(
    McpToolCache cache,
    ILogger<BeaconScrapeService> logger)
{
    private static readonly JsonSerializerOptions s_json = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private const string BeaconSearchUrl =
        "https://beacon.schneidercorp.com/application.aspx?app=RamseyCountyMN&PageType=Search";

    // ── Entry point ───────────────────────────────────────────────────────────

    public async Task<string> RunAsync(string csvPath, string outputDir, CancellationToken ct = default)
    {
        var mcp = new McpCaller(cache, logger);

        var addresses = await LoadAddressesAsync(mcp, csvPath, ct);
        if (addresses.Count == 0)
            return $"ERROR: No addresses found in {csvPath}";

        BeaconLog.Loaded(logger, addresses.Count, csvPath);

        var progressPath = Path.Combine(outputDir, "beacon-scrape-progress.json");
        var resultsPath  = Path.Combine(outputDir, "beacon-results.json");

        var progress     = await LoadProgressAsync(mcp, progressPath, ct);
        var results      = await LoadResultsAsync(mcp, resultsPath, ct);
        var completedSet = new HashSet<string>(progress.Completed, StringComparer.OrdinalIgnoreCase);
        var remaining    = addresses.Where(a => !completedSet.Contains(a)).ToList();

        BeaconLog.Progress(logger, completedSet.Count, remaining.Count);

        if (remaining.Count == 0)
            return $"All {addresses.Count} addresses already scraped. Results at {resultsPath}";

        // Navigate once to init the browser session and dismiss T&C
        var init = await InitBrowserAsync(mcp, ct);
        if (init.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
            return init;

        int scraped = 0, failed = 0;

        foreach (var address in remaining)
        {
            if (ct.IsCancellationRequested) break;

            BeaconLog.Scraping(logger, completedSet.Count + scraped + 1, addresses.Count, address);

            try
            {
                var result = await ScrapePropertyAsync(mcp, address, ct);
                results.Add(result);
                progress.Completed.Add(address);
                scraped++;

                await SaveResultsAsync(mcp, resultsPath,   results,  ct);
                await SaveProgressAsync(mcp, progressPath, progress, ct);

                BeaconLog.ScrapedOk(logger, address, result.Owner, result.Pin);
            }
            catch (Exception ex)
            {
                BeaconLog.ScrapedErr(logger, address, ex.Message);
                progress.Failed.Add(new FailedProperty(address, ex.Message));
                failed++;
                await SaveProgressAsync(mcp, progressPath, progress, ct);
            }

            if (!ct.IsCancellationRequested)
                await Task.Delay(1500, ct).ConfigureAwait(false);
        }

        // Close browser when done
        await mcp.CallAsync("CloseSession", [], ct);

        return $"Done. Scraped={scraped} Failed={failed} Total={progress.Completed.Count}/{addresses.Count}. {resultsPath}";
    }

    // ── Browser init ──────────────────────────────────────────────────────────

    private async Task<string> InitBrowserAsync(McpCaller mcp, CancellationToken ct)
    {
        var nav = await mcp.CallAsync("Navigate", new()
        {
            ["url"]       = BeaconSearchUrl,
            ["waitUntil"] = "domcontentloaded"
        }, ct);

        if (nav.Contains("\"success\":false", StringComparison.OrdinalIgnoreCase) ||
            nav.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
            return $"ERROR navigating to Beacon: {nav}";

        await Task.Delay(1200, ct);
        await DismissTAndC(mcp, ct);
        return "ok";
    }

    private async Task DismissTAndC(McpCaller mcp, CancellationToken ct)
    {
        try
        {
            await mcp.CallAsync("BrowserClick", new()
            {
                ["selector"]          = "text=Agree",
                ["waitForNavigation"] = false
            }, ct);
            await Task.Delay(400, ct);
        }
        catch { /* intermittent — not a failure */ }
    }

    // ── Per-property scrape ───────────────────────────────────────────────────

    private async Task<PropertyResult> ScrapePropertyAsync(McpCaller mcp, string address, CancellationToken ct)
    {
        await mcp.CallAsync("Navigate", new()
        {
            ["url"]       = BeaconSearchUrl,
            ["waitUntil"] = "domcontentloaded"
        }, ct);
        await Task.Delay(700, ct);
        await DismissTAndC(mcp, ct);

        await mcp.CallAsync("BrowserFill", new()
        {
            ["selector"]   = "#ctlBodyPane_ctl01_ctl01_txtAddress",
            ["value"]      = address,
            ["pressEnter"] = false
        }, ct);

        await mcp.CallAsync("BrowserClick", new()
        {
            ["selector"]          = "#ctlBodyPane_ctl01_ctl01_btnSearch",
            ["waitForNavigation"] = true
        }, ct);
        await Task.Delay(1500, ct);

        await mcp.CallAsync("BrowserClick", new()
        {
            ["selector"]          = "a[href*='KeyValue=']",
            ["waitForNavigation"] = true
        }, ct);
        await Task.Delay(2000, ct);

        var raw = await mcp.CallAsync("EvaluateJs", new()
        {
            ["script"] = @"(() => {
                const t = document.body.innerText;
                const m = (rx) => { const r = t.match(rx); return r ? r[1].trim() : 'N/A'; };
                return JSON.stringify({
                    pin:        m(/Parcel\s*(?:ID|Number|#)[\s\t:]+(\d[\d\- ]+)/i).replace(/\s+/g,''),
                    owner:      m(/Owner[\s\t]+([^\n\t]{2,80})/i),
                    market:     (() => { const r = t.match(/Estimated Market Value[\s\t]+\$?([\d,]+)/i); return r ? '$'+r[1] : 'N/A'; })(),
                    delinquent: (() => { const r = t.match(/Total Delinquent Taxes Due[\s\S]{0,60}?\$?([\d,]+\.\d{2})/i); return r ? '$'+r[1] : '$0.00'; })(),
                    forfeiture: m(/Forfeiture Year[\s\t:]+(\d{4})/i)
                });
            })()"
        }, ct);

        return ParseExtractedJson(address, raw);
    }

    // ── JS result parser ──────────────────────────────────────────────────────

    private PropertyResult ParseExtractedJson(string address, string raw)
    {
        string pin = "N/A", owner = "N/A", market = "N/A", delinquent = "$0.00", forfeiture = "N/A";
        try
        {
            // BrowserResult JSON comes back as the full BrowserResult object.
            // js_result is nested inside structured.js_result — unwrap it.
            var s = raw.Trim();

            // Try to parse as BrowserResult first
            using var outerDoc = JsonDocument.Parse(s);
            var root = outerDoc.RootElement;

            string? innerJson = null;

            // Path: structured.js_result (may be a string or object)
            if (root.TryGetProperty("structured", out var structured) &&
                structured.TryGetProperty("js_result", out var jsResult))
            {
                innerJson = jsResult.ValueKind == JsonValueKind.String
                    ? jsResult.GetString()
                    : jsResult.GetRawText();
            }
            // Fallback: result_json field
            else if (root.TryGetProperty("structured", out var structured2) &&
                     structured2.TryGetProperty("result_json", out var resultJson))
            {
                innerJson = resultJson.GetString();
            }

            if (string.IsNullOrWhiteSpace(innerJson))
                innerJson = s;

            // Strip outer quotes if double-serialized
            if (innerJson.StartsWith('"') && innerJson.EndsWith('"'))
                innerJson = JsonSerializer.Deserialize<string>(innerJson) ?? innerJson;

            var start = innerJson.IndexOf('{');
            var end   = innerJson.LastIndexOf('}');
            if (start < 0 || end <= start)
                return new PropertyResult(address, pin, owner, market, delinquent, forfeiture, DateTime.UtcNow.ToString("O"));

            using var doc = JsonDocument.Parse(innerJson[start..(end + 1)]);
            var r = doc.RootElement;
            if (r.TryGetProperty("pin",        out var p))  pin        = p.GetString() ?? "N/A";
            if (r.TryGetProperty("owner",      out var o))  owner      = o.GetString() ?? "N/A";
            if (r.TryGetProperty("market",     out var m))  market     = m.GetString() ?? "N/A";
            if (r.TryGetProperty("delinquent", out var d))  delinquent = d.GetString() ?? "$0.00";
            if (r.TryGetProperty("forfeiture", out var f))  forfeiture = f.GetString() ?? "N/A";
        }
        catch (Exception ex)
        {
            BeaconLog.ParseFailed(logger, address, ex.Message);
        }
        return new PropertyResult(address, pin, owner, market, delinquent, forfeiture, DateTime.UtcNow.ToString("O"));
    }

    // ── CSV / JSON persistence via Filesystem MCP ─────────────────────────────

    private static async Task<List<string>> LoadAddressesAsync(McpCaller mcp, string path, CancellationToken ct)
    {
        var raw = await mcp.CallAsync("ReadFile", new() { ["path"] = path }, ct);
        return raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim('\r', ' '))
            .Where(l => !string.IsNullOrWhiteSpace(l) &&
                        !l.Equals("address", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static async Task<ScrapeProgress> LoadProgressAsync(McpCaller mcp, string path, CancellationToken ct)
    {
        try
        {
            var raw   = await mcp.CallAsync("ReadFile", new() { ["path"] = path }, ct);
            var start = raw.IndexOf('{');
            if (start < 0) return new ScrapeProgress();
            return JsonSerializer.Deserialize<ScrapeProgress>(raw[start..], s_json) ?? new ScrapeProgress();
        }
        catch { return new ScrapeProgress(); }
    }

    private static async Task<List<PropertyResult>> LoadResultsAsync(McpCaller mcp, string path, CancellationToken ct)
    {
        try
        {
            var raw   = await mcp.CallAsync("ReadFile", new() { ["path"] = path }, ct);
            var start = raw.IndexOf('[');
            if (start < 0) return [];
            return JsonSerializer.Deserialize<List<PropertyResult>>(raw[start..], s_json) ?? [];
        }
        catch { return []; }
    }

    private static Task<string> SaveResultsAsync(McpCaller mcp, string path, List<PropertyResult> data, CancellationToken ct)
        => mcp.CallAsync("WriteFile", new()
        {
            ["path"]    = path,
            ["content"] = JsonSerializer.Serialize(data, s_json)
        }, ct);

    private static Task<string> SaveProgressAsync(McpCaller mcp, string path, ScrapeProgress data, CancellationToken ct)
        => mcp.CallAsync("WriteFile", new()
        {
            ["path"]    = path,
            ["content"] = JsonSerializer.Serialize(data, s_json)
        }, ct);
}
