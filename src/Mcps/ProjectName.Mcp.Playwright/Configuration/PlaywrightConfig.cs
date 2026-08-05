// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.PLAYWRIGHT
// File       : Configuration/PlaywrightConfig.cs
// Identity   : Browser automation sandbox and session configuration
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : PW-LAW-001 (URL safety), PW-LAW-003 (timeout caps),
//              PW-LAW-005 (serial page execution), ANTHROPIC-ACI-001
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design:
//   AllowedDomains is the structural URL gate — agents read playwright://config
//   to know which domains they can navigate before planning a scrape workflow.
//   Timeout caps are enforced in config, not per-call — the agent cannot
//   accidentally hang the browser by passing an unbounded timeout.
// ═══════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace ProjectName.Mcp.Playwright.Configuration;

/// <summary>
/// I Am the Playwright MCP configuration. I define the URL safety boundary,
/// session limits, and timeout contracts for all browser automation operations.
/// I enforce PW-LAW-001 (URL safety) structurally — tools never receive raw
/// unchecked URLs. I am injected as a singleton at startup.
/// </summary>
public sealed class PlaywrightConfig
{
    /// <summary>
    /// Allowed domain patterns. Empty list = all domains allowed (open mode).
    /// If set, any navigation to a domain not matching these patterns is blocked.
    /// Injected by Aspire: Playwright__AllowedDomains__0, __1, …
    /// Supports prefix match: "github.com" matches "github.com" and "api.github.com".
    /// </summary>
    public string[] AllowedDomains { get; init; } = [];

    /// <summary>
    /// Domains that are always blocked regardless of AllowedDomains.
    /// Structural Poka-yoke — prevents navigation to credential-harvesting or
    /// PII-exfiltration targets even when AllowedDomains is open.
    /// </summary>
    public string[] BlockedDomains { get; init; } =
    [
        "localhost",
        "127.0.0.1",
        "0.0.0.0",
        "169.254.*",  // link-local / cloud metadata
        "10.*",       // RFC1918 — block internal network navigation
        "192.168.*",
        "172.16.*",
    ];

    /// <summary>
    /// Maximum wall-clock milliseconds for page.GotoAsync (navigation timeout).
    /// PW-LAW-003: cap enforced here, not overridable by tool callers.
    /// Default: 30 seconds.
    /// </summary>
    public int NavigationTimeoutMs { get; init; } = 30_000;

    /// <summary>
    /// Maximum milliseconds for page.WaitForSelectorAsync and similar waits.
    /// Default: 10 seconds.
    /// </summary>
    public int SelectorTimeoutMs { get; init; } = 10_000;

    /// <summary>
    /// Maximum milliseconds for page.EvaluateAsync (JS evaluation).
    /// Default: 5 seconds.
    /// </summary>
    public int EvaluationTimeoutMs { get; init; } = 5_000;

    /// <summary>
    /// Maximum screenshot dimension in pixels (width or height).
    /// Prevents OOM from full-page captures of infinite-scroll pages.
    /// </summary>
    public int MaxScreenshotDimensionPx { get; init; } = 4096;

    /// <summary>
    /// Maximum page content length returned by playwright.get_page_content.
    /// Content beyond this limit is truncated with an explicit marker.
    /// Default: 200 KB (roughly 50K tokens of HTML).
    /// </summary>
    public int MaxContentLengthBytes { get; init; } = 200 * 1024;

    /// <summary>
    /// Run browser in headless mode. Default: true (server/CI safe).
    /// Set to false for local debugging only.
    /// </summary>
    public bool Headless { get; init; } = true;

    /// <summary>
    /// Browser channel: chromium (default, Patchright stealth), firefox, webkit.
    /// Patchright replaces Chromium binary for stealth — use "chromium" here.
    /// </summary>
    public string BrowserChannel { get; init; } = "chromium";

    /// <summary>
    /// Viewport width in pixels. Default: 1280 (standard desktop).
    /// </summary>
    public int ViewportWidth { get; init; } = 1280;

    /// <summary>
    /// Viewport height in pixels. Default: 720.
    /// </summary>
    public int ViewportHeight { get; init; } = 720;

    // ── URL safety gate (PW-LAW-001) ─────────────────────────────────────────

    /// <summary>
    /// Returns true if <paramref name="url"/> passes the safety gate:
    ///   — Must be http:// or https://
    ///   — Domain must not be in BlockedDomains
    ///   — If AllowedDomains is set, domain must match at least one entry
    /// This is the single URL choke-point — every navigation tool calls this.
    /// </summary>
    public bool IsUrlAllowed(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme is not "http" and not "https")
            return false;

        var host = uri.Host.ToLowerInvariant();

        // Block internal/metadata ranges
        if (BlockedDomains.Any(b => DomainPatternMatch(host, b)))
            return false;

        // AllowedDomains: if set, must match
        if (AllowedDomains.Length > 0)
            return AllowedDomains.Any(d => DomainPatternMatch(host, d));

        return true;
    }

    /// <summary>
    /// Returns a URL-safety violation message formatted for agent consumption.
    /// Includes AllowedDomains so the agent can self-correct without a separate call.
    /// </summary>
    public string UrlViolationMessage(string url) =>
        $"URL-SAFETY-VIOLATION: '{url}' is blocked. " +
        (AllowedDomains.Length > 0
            ? $"AllowedDomains: [{string.Join(", ", AllowedDomains)}]. "
            : "") +
        $"BlockedDomains include internal/metadata ranges. " +
        $"Read playwright://config for the full URL safety contract (PW-LAW-001).";

    private static bool DomainPatternMatch(string host, string pattern)
    {
        // Wildcard prefix: "10.*" matches "10.0.0.1", "10.128.x.x" etc.
        if (pattern.EndsWith(".*", StringComparison.Ordinal))
        {
            var prefix = pattern[..^2];
            return host.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
        // Suffix match: "github.com" matches "api.github.com"
        var lowerPattern = pattern.ToLowerInvariant();
        return string.Equals(host, lowerPattern, StringComparison.Ordinal) ||
               host.EndsWith($".{lowerPattern}", StringComparison.OrdinalIgnoreCase);
    }
}
