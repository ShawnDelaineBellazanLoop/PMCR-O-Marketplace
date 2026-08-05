// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.PLAYWRIGHT
// File       : Resources/PlaywrightResources.cs
// Identity   : Agent-Readable Resource Manifests
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, PW-LAW-001, ANTHROPIC-AGENT-001
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design — Resource manifest pattern:
//   Resources tell the agent WHAT EXISTS and WHAT TO DO NEXT — not just what's there.
//   playwright://skill        → inline SKILL.md (no filesystem dependency, always available)
//   playwright://config       → runtime-resolved config snapshot (AllowedDomains, timeouts)
//   playwright://session/status → current browser state for pre-flight checks
//   playwright://screenshot/latest → last captured screenshot (base64 PNG)
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using ProjectName.Mcp.Playwright.Configuration;

namespace ProjectName.Mcp.Playwright.Resources;

/// <summary>
/// I Am the Playwright MCP Resource Provider. I expose agent-readable manifests
/// that the orchestrator can read without opening a browser session.
/// All resources are TYPE 2 — read-only, no HIL required.
/// </summary>
[McpServerResourceType]
public sealed class PlaywrightResources(
    PlaywrightConfig config,
    PlaywrightSessionManager session)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // ── playwright://skill ────────────────────────────────────────────────────

    [McpServerResource(UriTemplate = "playwright://skill", Name = "Playwright SKILL.md",
        MimeType = "text/markdown")]
    [System.ComponentModel.Description(
        "Inline SKILL.md for the Playwright MCP server. " +
        "Read this first to understand tool contracts, TYPE 1/2 boundaries, " +
        "URL safety rules, and the Extract+Summarize agent pattern. " +
        "Always available — no browser session required.")]
    public TextResourceContents GetSkill()
    {
        return new TextResourceContents
        {
            Uri      = "playwright://skill",
            MimeType = "text/markdown",
            Text     = """
# PLAYWRIGHT MCP — SKILL.md
> Agent-readable quick reference. Always start here.

## Identity
Browser automation actuator for the PMCR-O cognitive stack.
Implements Anthropic Extract+Summarize: every tool return contains
`summary` (for reasoning), `structured` (for action), `next_actions` (for navigation).

## Tool Boundary

| Type   | Tools                                                       | HIL? |
|--------|-------------------------------------------------------------|------|
| TYPE 1 | navigate, click, fill, screenshot, evaluate, close_session  | YES  |
| TYPE 2 | get_session_status, get_page_content, get_url               | NO   |

## Pre-Flight Checklist (EC-002)
1. Read `playwright://config` — verify domain is in AllowedDomains
2. Read `playwright://session/status` — check if session is already open
3. Present TYPE 1 plan to orchestrator for approval
4. Call TYPE 1 tool — observe `summary` + `structured` return
5. Call `get_page_content` (TYPE 2) to extract and summarize result
6. Reflect — do `next_actions` indicate further steps?

## URL Safety (PW-LAW-001)
- Only http:// and https:// schemes allowed
- AllowedDomains whitelist enforced (empty list = unrestricted)
- BlockedDomains blacklist enforced at all times
- Use `playwright://config` to inspect current lists before planning

## Extract+Summarize Pattern (ANTHROPIC-AGENT-001)
```
result.summary        → embed directly in reasoning chain
result.structured     → field-addressable data (no re-parsing)
result.next_actions[] → explicit next steps for the agent
result.error          → set only on failure, includes self-correction hint
```

## Session Lifecycle
```
playwright.navigate  → opens browser automatically
playwright.get_page_content → extract + summarize (TYPE 2, no HIL)
playwright.click / fill → interact (TYPE 1, needs HIL)
playwright.screenshot → capture state (TYPE 1, needs HIL)
playwright.close_session → always call when done (TYPE 1, needs HIL)
```

## Resources
| URI                              | Contents                          |
|----------------------------------|-----------------------------------|
| playwright://skill               | This file                         |
| playwright://config              | Runtime config snapshot           |
| playwright://session/status      | Live browser session state        |
| playwright://screenshot/latest   | Last screenshot (base64 PNG)      |

## Prompts
- `playwright-navigate-plan`   → full agent loop for navigation + extraction
- `playwright-scrape-scaffold` → multi-page scraping workflow
- `playwright-debug-failure`   → diagnose selector/navigation failures
""",
        };
    }

    // ── playwright://config ───────────────────────────────────────────────────

    [McpServerResource(UriTemplate = "playwright://config", Name = "Playwright Config",
        MimeType = "application/json")]
    [System.ComponentModel.Description(
        "Runtime-resolved Playwright MCP configuration snapshot. " +
        "Read before any TYPE 1 tool to verify AllowedDomains, timeouts, and headless mode. " +
        "No browser session required.")]
    public TextResourceContents GetConfig()
    {
        var snapshot = new
        {
            allowed_domains         = config.AllowedDomains,
            blocked_domains         = config.BlockedDomains,
            headless                = config.Headless,
            navigation_timeout_ms   = config.NavigationTimeoutMs,
            selector_timeout_ms     = config.SelectorTimeoutMs,
            evaluation_timeout_ms   = config.EvaluationTimeoutMs,
            max_content_length_bytes = config.MaxContentLengthBytes,
            browser_channel         = config.BrowserChannel,
            note = config.AllowedDomains.Length == 0
                ? "AllowedDomains is empty — all domains permitted (development mode)"
                : $"{config.AllowedDomains.Length} domain(s) whitelisted",
            agent_guidance = new[]
            {
                "If your target domain is not in allowed_domains, the navigate tool will reject it",
                "Set PLAYWRIGHT__AllowedDomains in environment to restrict domains",
                "navigation_timeout_ms applies to playwright.navigate and playwright.click (waitForNavigation)",
                "selector_timeout_ms applies to playwright.click and playwright.fill",
            },
        };

        return new TextResourceContents
        {
            Uri      = "playwright://config",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(snapshot, JsonOptions),
        };
    }

    // ── playwright://session/status ───────────────────────────────────────────

    [McpServerResource(UriTemplate = "playwright://session/status", Name = "Browser Session Status",
        MimeType = "application/json")]
    [System.ComponentModel.Description(
        "Live browser session state. Read before any TYPE 1 tool as pre-flight check. " +
        "Returns is_open, current_url, page_title, navigation_count, last_error. " +
        "TYPE 2 — no browser launched, no HIL required.")]
    public TextResourceContents GetSessionStatus()
    {
        var snap = session.GetStatusSnapshot();
        return new TextResourceContents
        {
            Uri      = "playwright://session/status",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(snap, JsonOptions),
        };
    }

    // ── playwright://screenshot/latest ───────────────────────────────────────

    [McpServerResource(UriTemplate = "playwright://screenshot/latest", Name = "Latest Screenshot",
        MimeType = "application/json")]
    [System.ComponentModel.Description(
        "Base64-encoded PNG of the last captured screenshot. " +
        "Available after playwright.screenshot has been called. " +
        "Returns null if no screenshot captured in current session.")]
    public TextResourceContents GetLatestScreenshot()
    {
        var base64 = session.GetLastScreenshot();
        var result = new
        {
            available     = base64 is not null,
            format        = "png",
            base64_png    = base64,
            agent_guidance = base64 is null
                ? "No screenshot available — call playwright.screenshot first"
                : "Pass base64_png to a vision model for visual analysis, or decode to inspect",
        };

        return new TextResourceContents
        {
            Uri      = "playwright://screenshot/latest",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(result, JsonOptions),
        };
    }
}
