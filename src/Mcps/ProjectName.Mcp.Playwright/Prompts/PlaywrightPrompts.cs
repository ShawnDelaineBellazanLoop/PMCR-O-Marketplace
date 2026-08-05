// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.PLAYWRIGHT
// File       : Prompts/PlaywrightPrompts.cs
// Identity   : Agent Loop Scaffolds (Plan → Act → Observe → Reflect)
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, ANTHROPIC-AGENT-001
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design — Prompt scaffold pattern:
//   Prompts encode the FULL AGENT LOOP, not just pre-flight checklists.
//   Each prompt returns a ChatMessage sequence that walks the agent through:
//     PLAN   → what to read/check before acting
//     ACT    → the TYPE 1 tool call with HIL gate
//     OBSERVE→ the Extract+Summarize call (TYPE 2) after acting
//     REFLECT→ next_actions evaluation and loop decision
// ═══════════════════════════════════════════════════════════════════════════════

using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace ProjectName.Mcp.Playwright.Prompts;

/// <summary>
/// I Am the Playwright MCP Prompt Provider. I scaffold complete agent loops
/// for navigation, scraping, and debugging. My prompts encode the full
/// Plan → Act → Observe → Reflect cycle with explicit HIL gates.
/// </summary>
[McpServerPromptType]
public sealed class PlaywrightPrompts
{
    // ── playwright-navigate-plan ──────────────────────────────────────────────

    [McpServerPrompt(Name = "playwright-navigate-plan")]
    [Description(
        "Full agent loop scaffold for navigating to a URL and extracting structured content. " +
        "Encodes the Plan→Act→Observe→Reflect cycle with HIL gates per EC-002. " +
        "Provide: target_url, extraction_goal.")]
    public static IEnumerable<ChatMessage> NavigatePlan(
        [Description("The URL to navigate to.")] string targetUrl,
        [Description("What the agent should extract or accomplish on the page.")] string extractionGoal)
    {
        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Playwright MCP — Navigation Plan Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, PW-LAW-001, ANTHROPIC-AGENT-001

                ══ PLAN (pre-flight — TYPE 2, no HIL) ════════════════════════════════════
                1. Read playwright://config
                   Verify: Is "{targetUrl}" in AllowedDomains (or AllowedDomains is empty)?
                   Verify: Are timeouts acceptable for this task?
                2. Read playwright://session/status
                   If is_open=true AND current_url matches domain: consider skipping navigate
                   If is_open=false: navigate will open browser automatically

                ══ TARGET ═════════════════════════════════════════════════════════════════
                URL   : {targetUrl}
                Goal  : {extractionGoal}

                ══ ACT (HIL approval required per EC-002) ══════════════════════════════════
                Step 1 — playwright.navigate(url="{targetUrl}", waitUntil="domcontentloaded")
                         TYPE 1 — Orchestrator + HIL approval required before calling

                ══ OBSERVE (TYPE 2 — no HIL, call after navigate) ══════════════════════════
                Step 2 — playwright.get_page_content(includeRawHtml=false)
                         Read from result:
                           .summary       → embed in reasoning chain
                           .structured.headings[]    → page structure
                           .structured.links[]       → navigation targets
                           .structured.forms[]       → interactive elements
                           .structured.text_chunks[] → content for goal: "{extractionGoal}"
                           .next_actions             → explicit continuation hints

                Step 3 — playwright.screenshot(fullPage=false)  [optional, TYPE 1]
                         Use only if visual verification of the page state is needed

                ══ REFLECT ══════════════════════════════════════════════════════════════════
                Does text_chunks[] satisfy goal: "{extractionGoal}"?
                  YES → Summarize findings, call playwright.close_session (TYPE 1, HIL)
                  NO  → Follow next_actions[]:
                    If links[] contain relevant targets → navigate to next URL (TYPE 1, loop)
                    If forms[] present → playwright.fill + playwright.click (TYPE 1)
                    If more content needed → playwright.evaluate for custom extraction (TYPE 1)

                ══ CLEANUP ══════════════════════════════════════════════════════════════════
                playwright.close_session — always call when workflow is complete (TYPE 1, HIL)
                """),

            new ChatMessage(ChatRole.User,
                $"Navigate to {targetUrl} and: {extractionGoal}"),

            new ChatMessage(ChatRole.Assistant,
                $"""
                I'll navigate to {targetUrl} and extract: {extractionGoal}

                Pre-flight reads (TYPE 2 — no HIL required):
                  1. Read playwright://config — verify domain allowance and timeouts
                  2. Read playwright://session/status — check current browser state

                Planned steps (all TYPE 1 require HIL approval before dispatch):
                """),
        ];
    }

    // ── playwright-scrape-scaffold ────────────────────────────────────────────

    [McpServerPrompt(Name = "playwright-scrape-scaffold")]
    [Description(
        "Multi-page scraping workflow scaffold. " +
        "Encodes the pagination/link-follow loop with extract+summarize at each step. " +
        "Provide: start_url, data_to_collect, max_pages.")]
    public static IEnumerable<ChatMessage> ScrapeScaffold(
        [Description("Starting URL for the scrape.")] string startUrl,
        [Description("Description of data to collect across pages.")] string dataToCollect,
        [Description("Maximum number of pages to visit. Default: 5.")] int maxPages = 5)
    {
        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Playwright MCP — Multi-Page Scrape Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, PW-LAW-001, ANTHROPIC-AGENT-001

                ══ LOOP ARCHITECTURE (Plan→Act→Observe→Reflect × N pages) ═════════════════

                State: visited=[], collected=[], queue=["{startUrl}"], page_count=0

                FOR EACH URL in queue (while page_count < {maxPages}):

                  PLAN (TYPE 2, no HIL):
                    1. Read playwright://session/status — verify session state
                    2. Check: Is URL in AllowedDomains? (playwright://config)
                    3. Check: Already in visited[]? Skip if yes.

                  ACT (TYPE 1 — HIL gate per EC-002):
                    playwright.navigate(url=current_url, waitUntil="domcontentloaded")

                  OBSERVE (TYPE 2, no HIL):
                    result = playwright.get_page_content()
                    Extract from result.structured:
                      .text_chunks[] → scan for: "{dataToCollect}"
                      .links[]       → find pagination links ("next", "page 2", etc.)
                      .headings[]    → identify section boundaries
                      .forms[]       → detect search/filter forms
                    Append matches to collected[]
                    Append pagination links to queue[]
                    page_count++

                  REFLECT:
                    Stopping conditions (any): page_count >= {maxPages} | queue empty | goal met
                    Continue: queue has unvisited pagination links AND goal not yet satisfied

                ══ TARGETS ════════════════════════════════════════════════════════════════
                Start URL   : {startUrl}
                Collect     : {dataToCollect}
                Page limit  : {maxPages}

                ══ OUTPUT CONTRACT ════════════════════════════════════════════════════════
                After loop completes:
                  1. Summarize collected[] into structured report
                  2. Report: pages_visited, items_collected, urls_skipped
                  3. playwright.close_session (TYPE 1, HIL required)

                ══ ANTI-PATTERNS ══════════════════════════════════════════════════════════
                Do NOT navigate to already-visited URLs
                Do NOT extract raw_html unless structured extraction fails
                Do NOT exceed {maxPages} pages without re-confirming with orchestrator
                Do NOT leave session open after workflow completes
                """),

            new ChatMessage(ChatRole.User,
                $"Scrape up to {maxPages} pages starting at {startUrl} to collect: {dataToCollect}"),

            new ChatMessage(ChatRole.Assistant,
                $"""
                I'll scrape up to {maxPages} pages from {startUrl} to collect: {dataToCollect}

                Pre-flight reads (TYPE 2 — no HIL required):
                  1. Read playwright://config — verify AllowedDomains and timeouts
                  2. Read playwright://session/status — check current browser state

                Loop plan (each navigate is TYPE 1, requires HIL approval):
                """),
        ];
    }

    // ── playwright-debug-failure ──────────────────────────────────────────────

    [McpServerPrompt(Name = "playwright-debug-failure")]
    [Description(
        "Debug scaffold for Playwright tool failures — selector not found, navigation timeout, JS error. " +
        "Provide: failed_tool, error_message, last_url.")]
    public static IEnumerable<ChatMessage> DebugFailure(
        [Description("The tool that failed, e.g. playwright.click")] string failedTool,
        [Description("The error message from the failed tool result.")] string errorMessage,
        [Description("The URL that was open when the failure occurred.")] string lastUrl)
    {
        var diagnosis = errorMessage switch
        {
            var e when e.Contains("not found") && e.Contains("Selector") =>
                "Selector not found — use get_page_content to inspect available forms[] and selectors",
            var e when e.Contains("Timeout") || e.Contains("timeout") =>
                "Navigation or selector timeout — check playwright://config for timeout limits, retry with waitUntil=load",
            var e when e.Contains("ERR_NAME_NOT_RESOLVED") =>
                "DNS resolution failure — verify URL spelling and network connectivity",
            var e when e.Contains("ERR_CONNECTION_REFUSED") =>
                "Connection refused — server may be down or wrong port",
            var e when e.Contains("not in allowed") =>
                "Domain blocked — read playwright://config AllowedDomains, update PLAYWRIGHT__AllowedDomains env var",
            var e when e.Contains("No browser session") =>
                "Session closed — call playwright.navigate first to open a session",
            _ =>
                "Unknown error — read playwright://config and playwright://session/status for context",
        };

        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Playwright MCP — Failure Debug Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, PRODUCT-002, ANTHROPIC-AGENT-001

                You are the Checker. A Playwright tool call failed. Do not hallucinate success.
                PRODUCT-002: null (LOOP) over hallucination. Always.

                ══ Failure Summary ════════════════════════════════════════════════════════
                Tool     : {failedTool}
                Error    : {errorMessage}
                URL      : {lastUrl}
                Diagnosis: {diagnosis}

                ══ Diagnosis Tree (TYPE 2 reads — no HIL) ═══════════════════════════════
                Step 1 — Read playwright://session/status
                  is_open=false?        → session was closed; re-run playwright.navigate first
                  last_error set?       → read it for root cause
                  current_url wrong?    → page changed unexpectedly; re-navigate to {lastUrl}

                Step 2 — playwright.get_page_content(includeRawHtml=false)  [TYPE 2]
                  Check .headings[]    → did the page load correctly?
                  Check .forms[]       → does the target selector still exist?
                  Check .links[]       → did the page redirect?
                  Check .text_chunks[] → is there an error/CAPTCHA message on the page?

                Step 3 — playwright.screenshot(fullPage=true)  [TYPE 1, HIL required]
                  Visual confirmation of current page state
                  Check for CAPTCHA, login wall, or error page

                ══ Fix Patterns ════════════════════════════════════════════════════════════
                Selector not found:
                  Use get_page_content forms[] to find correct input selectors
                  Try text selector syntax: text=Submit instead of button[type=submit]
                Navigation timeout:
                  Retry with waitUntil=load (slower but more reliable than domcontentloaded)
                  Check playwright://config navigation_timeout_ms
                Domain blocked / not in allowed:
                  Read playwright://config to see AllowedDomains list
                  Set PLAYWRIGHT__AllowedDomains env var to include the target domain
                Session closed:
                  playwright.navigate will open a new session automatically

                ══ Recovery Steps ══════════════════════════════════════════════════════════
                1. Apply fix identified above
                2. playwright.navigate(url="{lastUrl}") — reload from known state
                3. playwright.get_page_content() — verify page loaded correctly
                4. Retry {failedTool} with corrected parameters

                ══ Checker Scoring ══════════════════════════════════════════════════════════
                COMPLETENESS: Did the tool produce any usable data?
                CORRECTNESS : Did the operation match the plan intent?
                COMPLIANCE  : Was the sandbox respected? Was HIL present for TYPE 1?

                Verdict: Issue LOOP with specific corrected parameters, or ESCALATE after 3 loops.
                """),

            new ChatMessage(ChatRole.User,
                $"{failedTool} failed: {errorMessage} (at {lastUrl}). Produce debug verdict."),
        ];
    }
}
