// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.PLAYWRIGHT
// File       : Configuration/PlaywrightSessionManager.cs
// Identity   : Patchright browser lifecycle manager — single session, serial execution
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : PW-LAW-005 (serial page execution — one page at a time),
//              ANTHROPIC-ACI-001 (structured state — agent always knows session state)
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design:
//   Session state is always visible via playwright://session/status.
//   The agent never guesses whether a browser is open — it reads the resource.
//   GetStatusSnapshot() returns a structured record the agent can reason over,
//   including current_url, title, and a list of next_actions.
//   Serial execution is enforced by SemaphoreSlim — no concurrent page ops (PW-LAW-005).
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace ProjectName.Mcp.Playwright.Configuration;

/// <summary>
/// I Am the Playwright Session Manager. I own the single Patchright browser instance
/// for this MCP server. I enforce serial page execution (PW-LAW-005) via a semaphore.
/// I expose GetStatusSnapshot() so agents always have a structured, accurate view of
/// the session state before issuing any browser command (ANTHROPIC-AGENT-001).
/// </summary>
public sealed class PlaywrightSessionManager : IAsyncDisposable
{
    private readonly PlaywrightConfig _config;
    private readonly ILogger<PlaywrightSessionManager> _logger;

    private IPlaywright? _playwright;
    private IBrowser?    _browser;
    private IPage?       _page;
    private string?      _lastScreenshotBase64;
    private string?      _lastError;
    private DateTimeOffset _sessionStartedAt;
    private int          _navigationCount;

    // PW-LAW-005: serial execution — one page operation at a time
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly Action<ILogger, Exception?> _logLaunch =
        LoggerMessage.Define(LogLevel.Information, new EventId(30, "Launch"), "[PW] Browser launched (Patchright/Chromium)");
    private static readonly Action<ILogger, Exception?> _logClose =
        LoggerMessage.Define(LogLevel.Information, new EventId(31, "Close"), "[PW] Browser closed");
    private static readonly Action<ILogger, string, Exception?> _logNav =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(32, "Nav"), "[PW] Navigate → {Url}");
    private static readonly Action<ILogger, string, Exception?> _logFault =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(39, "Fault"), "[PW] Fault: {Msg}");

    public PlaywrightSessionManager(PlaywrightConfig config, ILogger<PlaywrightSessionManager> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ── Session state ─────────────────────────────────────────────────────────

    public bool IsOpen => _browser is { IsConnected: true } && _page is not null;

    /// <summary>
    /// Returns a structured snapshot of session state.
    /// Agents read playwright://session/status (which calls this) before any tool call.
    /// Includes current_url, page_title, is_open, navigation_count, and next_actions.
    /// </summary>
    public SessionSnapshot GetStatusSnapshot()
    {
        string? currentUrl   = null;
        string? title        = null;

        if (IsOpen && _page is not null)
        {
            try { currentUrl = _page.Url; } catch { /* page may be navigating */ }
            try { title      = _page.TitleAsync().GetAwaiter().GetResult(); } catch { }
        }

        return new SessionSnapshot
        {
            IsOpen           = IsOpen,
            CurrentUrl       = currentUrl,
            PageTitle        = title,
            NavigationCount  = _navigationCount,
            SessionStartedAt = IsOpen ? _sessionStartedAt : null,
            LastError        = _lastError,
            Config = new
            {
                headless          = _config.Headless,
                browser_channel   = _config.BrowserChannel,
                viewport          = $"{_config.ViewportWidth}x{_config.ViewportHeight}",
                navigation_timeout_ms = _config.NavigationTimeoutMs,
                selector_timeout_ms   = _config.SelectorTimeoutMs,
                allowed_domains   = _config.AllowedDomains.Length > 0
                    ? (object)_config.AllowedDomains
                    : "all (open mode)",
            },
            NextActions = IsOpen
                ? new[]
                {
                    $"Use playwright.navigate to go to a URL (current: {currentUrl ?? "none"})",
                    "Use playwright.get_page_content to extract structured page data",
                    "Use playwright.screenshot to capture the current state",
                    "Use playwright.close_session to close the browser when done",
                }
                : new[]
                {
                    "Use playwright.navigate to open a URL (this will also launch the browser)",
                    "Read playwright://config to verify allowed domains before navigating",
                },
        };
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Ensures the browser and a single page are open.
    /// Idempotent — safe to call before every tool operation.
    /// Uses Patchright for stealth anti-bot bypass.
    /// </summary>
    public async Task EnsureOpenAsync(CancellationToken cancellationToken = default)
    {
        if (IsOpen) return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (IsOpen) return; // double-check after lock

            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            // Use Patchright.CreateAsync() if Patchright NuGet replaces the binary.
            // With the Patchright NuGet package the entry point is the same Playwright API.
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _config.Headless,
                Channel  = _config.BrowserChannel == "chromium" ? null : _config.BrowserChannel,
            });

            _page = await _browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width  = _config.ViewportWidth,
                    Height = _config.ViewportHeight,
                },
            });

            _page.SetDefaultNavigationTimeout(_config.NavigationTimeoutMs);
            _page.SetDefaultTimeout(_config.SelectorTimeoutMs);

            _sessionStartedAt = DateTimeOffset.UtcNow;
            _navigationCount  = 0;
            _lastError        = null;

            _logLaunch(_logger, null);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Acquires the serial page lock and executes <paramref name="operation"/>.
    /// PW-LAW-005: all page operations are serialised through this method.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<IPage, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await EnsureOpenAsync(cancellationToken);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_page is null) throw new InvalidOperationException("Page is null after EnsureOpenAsync.");
            return await operation(_page);
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            _logFault(_logger, ex.Message, ex);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Navigates to <paramref name="url"/> and increments the navigation counter.</summary>
    public async Task<IResponse?> NavigateAsync(string url, CancellationToken cancellationToken = default)
    {
        _logNav(_logger, url, null);
        var response = await ExecuteAsync(p => p.GotoAsync(url, new PageGotoOptions
        {
            Timeout     = _config.NavigationTimeoutMs,
            WaitUntil   = WaitUntilState.DOMContentLoaded,
        }), cancellationToken);
        _navigationCount++;
        return response;
    }

    /// <summary>Stores the latest screenshot for the playwright://screenshot/latest resource.</summary>
    public void SetLastScreenshot(string base64Png) => _lastScreenshotBase64 = base64Png;

    /// <summary>Returns the latest screenshot base64, or null if none taken.</summary>
    public string? GetLastScreenshot() => _lastScreenshotBase64;

    /// <summary>Closes the browser and resets all session state.</summary>
    public async Task CloseAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _logClose(_logger, null);
            if (_browser is not null) await _browser.CloseAsync();
            _playwright?.Dispose();
            _page             = null;
            _browser          = null;
            _playwright       = null;
            _lastScreenshotBase64 = null;
            _lastError        = null;
            _navigationCount  = 0;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { await CloseAsync(); } catch { /* best-effort on dispose */ }
        _lock.Dispose();
    }
}

/// <summary>
/// Structured session state snapshot returned by GetStatusSnapshot().
/// Agents read this via playwright://session/status before every tool call.
/// </summary>
public sealed class SessionSnapshot
{
    public bool IsOpen { get; init; }
    public string? CurrentUrl { get; init; }
    public string? PageTitle { get; init; }
    public int NavigationCount { get; init; }
    public DateTimeOffset? SessionStartedAt { get; init; }
    public string? LastError { get; init; }
    public object? Config { get; init; }
    public string[] NextActions { get; init; } = [];
}
