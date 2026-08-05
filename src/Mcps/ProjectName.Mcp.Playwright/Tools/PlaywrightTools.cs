// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.PLAYWRIGHT
// File       : Tools/PlaywrightTools.cs
// Identity   : Browser Automation Actuator — augmented returns with extract+summarize
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, PW-LAW-001, PW-LAW-003, PW-LAW-005, ANTHROPIC-AGENT-001
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design — Extract + Summarize pattern:
//   Every tool return includes:
//     success        — boolean gate
//     summary        — one-sentence natural language for agent reasoning chain
//     structured     — typed extracted data (links, headings, forms, meta, text_chunks)
//     raw_html       — verbatim when agent needs it (optional, may be truncated)
//     next_actions   — explicit "what to do next" (ANTHROPIC-AGENT-001)
//   playwright.get_page_content extracts AND summarizes in one call:
//     the agent gets headings, links, text_chunks, and a word_count without
//     needing a separate "summarize this HTML" LLM call.
// ═══════════════════════════════════════════════════════════════════════════════

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ModelContextProtocol.Server;
using ProjectName.Mcp.Playwright.Configuration;

using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ProjectName.Mcp.Playwright.Tools;

/// <summary>
/// I Am the Playwright MCP Tool Provider. I am the "Browser Hands" of the PMCR-O
/// cognitive stack. I expose browser automation with augmented returns — every
/// result includes structured extraction and a summary the agent can reason over
/// without re-processing raw HTML. I enforce serial execution (PW-LAW-005) and
/// URL safety (PW-LAW-001) by construction.
/// </summary>
[McpServerToolType]
public sealed class PlaywrightTools(
    PlaywrightConfig config,
    PlaywrightSessionManager session,
    ILogger<PlaywrightTools> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly Action<ILogger, string, Exception?> _logNav =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(40, "Nav"), "[PW] navigate → {Url}");
    private static readonly Action<ILogger, string, Exception?> _logClick =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(41, "Click"), "[PW] click: {Selector}");
    private static readonly Action<ILogger, string, Exception?> _logFill =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(42, "Fill"), "[PW] fill: {Selector}");
    private static readonly Action<ILogger, Exception?> _logShot =
        LoggerMessage.Define(LogLevel.Information, new EventId(43, "Shot"), "[PW] screenshot");
    private static readonly Action<ILogger, string, Exception?> _logFault =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(49, "Fault"), "[PW] fault: {Msg}");

    // ════════════════════════════════════════════════════════════════════════
    // TYPE 1 — World-changing / network-touching tools
    // Orchestrator + HIL approval required (EC-002, MAAI-001).
    // ════════════════════════════════════════════════════════════════════════

    [McpServerTool(Name = "playwright.navigate")]
    [Description(
        "TYPE 1 — Navigate the browser to a URL. Orchestrator + HIL approval required (EC-002, MAAI-001). " +
        "URL safety enforced: must be http/https, not blocked (PW-LAW-001). " +
        "Returns augmented BrowserResult with: summary, structured (title, url, status_code, " +
        "redirect_chain), and next_actions. " +
        "Launches the browser automatically if not open. " +
        "Read playwright://config for AllowedDomains before planning navigation.")]
    public async Task<BrowserResult> Navigate(
        [Description("Absolute URL to navigate to. Must be http:// or https://")] string url,
        [Description("Wait strategy: domcontentloaded (default, fast) | load (all resources) | networkidle (SPAs)")] string waitUntil = "domcontentloaded",
        CancellationToken cancellationToken = default)
    {
        if (!config.IsUrlAllowed(url))
            return Err(config.UrlViolationMessage(url));

        _logNav(logger, url, null);

        try
        {
            var waitState = waitUntil switch
            {
                "load"        => WaitUntilState.Load,
                "networkidle" => WaitUntilState.NetworkIdle,
                _             => WaitUntilState.DOMContentLoaded,
            };

            IResponse? response = null;
            string title = string.Empty;
            string finalUrl = url;

            await session.ExecuteAsync(async page =>
            {
                response = await page.GotoAsync(url, new PageGotoOptions
                {
                    Timeout   = config.NavigationTimeoutMs,
                    WaitUntil = waitState,
                });
                title    = await page.TitleAsync();
                finalUrl = page.Url;
                return true;
            }, cancellationToken);

            var statusCode = response?.Status ?? 0;

            return new BrowserResult
            {
                Success    = statusCode is 0 or >= 200 and < 400,
                Summary    = $"Navigated to '{title}' ({finalUrl}) — HTTP {statusCode}.",
                Structured = new
                {
                    url          = finalUrl,
                    requested_url = url,
                    title,
                    status_code  = statusCode,
                    ok           = response?.Ok ?? false,
                    wait_until   = waitUntil,
                    navigated_at_utc = DateTimeOffset.UtcNow,
                },
                NextActions =
                [
                    "Use playwright.get_page_content to extract structured page data and text",
                    "Use playwright.screenshot to capture the visual state",
                    "Use playwright.click or playwright.fill to interact with the page",
                ],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Navigate fault: {ex.Message}. URL: {url}");
        }
    }

    [McpServerTool(Name = "playwright.click")]
    [Description(
        "TYPE 1 — Click an element on the current page. Orchestrator + HIL approval required. " +
        "Uses CSS selector or text selector (text='Submit'). " +
        "Returns structured result with element_found, element_text, and page state after click. " +
        "Use playwright.get_page_content first to identify the correct selector (TYPE 2, no HIL).")]
    public async Task<BrowserResult> Click(
        [Description("CSS selector or Playwright text selector, e.g. 'button[type=submit]' or 'text=Login'.")] string selector,
        [Description("If true, wait for navigation to complete after click. Default: false.")] bool waitForNavigation = false,
        CancellationToken cancellationToken = default)
    {
        if (!session.IsOpen)
            return Err("No browser session open. Use playwright.navigate first to open a page.");

        _logClick(logger, selector, null);

        try
        {
            string elementText  = string.Empty;
            string urlAfter     = string.Empty;
            string titleAfter   = string.Empty;

            await session.ExecuteAsync(async page =>
            {
                var element = await page.QuerySelectorAsync(selector);
                if (element is null)
                    throw new InvalidOperationException($"Selector not found: '{selector}'");

                elementText = await element.InnerTextAsync() ?? string.Empty;

                if (waitForNavigation)
                {
                    // WaitForNavigationAsync is obsolete — use WaitForLoadStateAsync instead
                    var clickTask = element.ClickAsync();
                    var navTask   = page.WaitForLoadStateAsync(LoadState.DOMContentLoaded,
                        new PageWaitForLoadStateOptions { Timeout = config.NavigationTimeoutMs });
                    await Task.WhenAll(clickTask, navTask);
                }
                else
                {
                    await element.ClickAsync();
                }

                urlAfter   = page.Url;
                titleAfter = await page.TitleAsync();
                return true;
            }, cancellationToken);

            return new BrowserResult
            {
                Success    = true,
                Summary    = $"Clicked '{selector}' (text: '{elementText.Trim().TruncateTo(80)}') — page: '{titleAfter}'.",
                Structured = new
                {
                    selector,
                    element_text   = elementText.Trim(),
                    url_after      = urlAfter,
                    title_after    = titleAfter,
                    navigation_occurred = waitForNavigation,
                    clicked_at_utc = DateTimeOffset.UtcNow,
                },
                NextActions =
                [
                    "Use playwright.get_page_content to read updated page state",
                    "Use playwright.screenshot to capture result",
                    "Use playwright.fill if a form appeared after click",
                ],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Click fault: {ex.Message}. " +
                       $"If selector not found, use playwright.get_page_content to inspect available selectors.");
        }
    }

    [McpServerTool(Name = "playwright.fill")]
    [Description(
        "TYPE 1 — Fill a text input or textarea. Orchestrator + HIL approval required. " +
        "Clears existing value before filling. " +
        "Returns structured result with field_label (if detected) and value_length. " +
        "Use playwright.get_page_content first to identify form field selectors.")]
    public async Task<BrowserResult> Fill(
        [Description("CSS selector for the input element, e.g. 'input[name=email]' or '#search'.")] string selector,
        [Description("Value to fill into the field. Clears existing value first.")] string value,
        [Description("If true, press Enter after filling (submit pattern). Default: false.")] bool pressEnter = false,
        CancellationToken cancellationToken = default)
    {
        if (!session.IsOpen)
            return Err("No browser session open. Use playwright.navigate first.");

        _logFill(logger, selector, null);

        try
        {
            string labelText = string.Empty;
            string urlAfter  = string.Empty;

            await session.ExecuteAsync(async page =>
            {
                // Attempt to find associated label for agent-readable context
                try
                {
                    var el = await page.QuerySelectorAsync(selector);
                    if (el is not null)
                    {
                        var id = await el.GetAttributeAsync("id");
                        if (!string.IsNullOrEmpty(id))
                        {
                            var label = await page.QuerySelectorAsync($"label[for='{id}']");
                            if (label is not null) labelText = await label.InnerTextAsync() ?? string.Empty;
                        }
                    }
                }
                catch { /* label detection is best-effort */ }

                await page.FillAsync(selector, value, new PageFillOptions
                {
                    Timeout = config.SelectorTimeoutMs,
                });

                if (pressEnter)
                    await page.PressAsync(selector, "Enter");

                urlAfter = page.Url;
                return true;
            }, cancellationToken);

            return new BrowserResult
            {
                Success    = true,
                Summary    = $"Filled '{selector}'{(string.IsNullOrEmpty(labelText) ? "" : $" (label: '{labelText.Trim()}')")} with {value.Length} chars{(pressEnter ? ", pressed Enter" : "")}.",
                Structured = new
                {
                    selector,
                    label          = labelText.Trim(),
                    value_length   = value.Length,
                    enter_pressed  = pressEnter,
                    url_after      = urlAfter,
                    filled_at_utc  = DateTimeOffset.UtcNow,
                },
                NextActions = pressEnter
                    ? ["Use playwright.get_page_content to read the result page", "Use playwright.screenshot to capture state"]
                    : ["Use playwright.click to submit the form", "Fill additional fields before submitting"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Fill fault: {ex.Message}. Use playwright.get_page_content to verify field selector.");
        }
    }

    [McpServerTool(Name = "playwright.screenshot")]
    [Description(
        "TYPE 1 — Capture a PNG screenshot of the current page. Orchestrator + HIL approval required. " +
        "Returns base64-encoded PNG and structured metadata (width, height, file_size_bytes). " +
        "Screenshot stored for playwright://screenshot/latest resource. " +
        "Use fullPage=false for viewport-only capture (faster, smaller).")]
    public async Task<BrowserResult> Screenshot(
        [Description("If true, capture full scrollable page height. Default: false (viewport only).")] bool fullPage = false,
        [Description("Optional CSS selector to capture only that element.")] string? elementSelector = null,
        CancellationToken cancellationToken = default)
    {
        if (!session.IsOpen)
            return Err("No browser session open. Navigate to a page first.");

        _logShot(logger, null);

        try
        {
            byte[] bytes = [];
            string urlAt = string.Empty;

            await session.ExecuteAsync(async page =>
            {
                urlAt = page.Url;
                if (!string.IsNullOrEmpty(elementSelector))
                {
                    var el = await page.QuerySelectorAsync(elementSelector);
                    bytes  = el is not null
                        ? await el.ScreenshotAsync()
                        : await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = fullPage });
                }
                else
                {
                    bytes = await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = fullPage });
                }
                return true;
            }, cancellationToken);

            var base64 = Convert.ToBase64String(bytes);
            session.SetLastScreenshot(base64);

            return new BrowserResult
            {
                Success    = true,
                Summary    = $"Screenshot captured — {bytes.Length:N0} bytes, {(fullPage ? "full page" : "viewport")}, URL: {urlAt}.",
                Structured = new
                {
                    url_at              = urlAt,
                    full_page           = fullPage,
                    element_selector    = elementSelector,
                    file_size_bytes     = bytes.Length,
                    format              = "png",
                    base64_png          = base64,
                    captured_at_utc     = DateTimeOffset.UtcNow,
                },
                NextActions =
                [
                    "Read playwright://screenshot/latest to retrieve the image without re-capturing",
                    "Pass base64_png to a vision model for visual analysis",
                ],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Screenshot fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "playwright.evaluate")]
    [Description(
        "TYPE 1 — Execute JavaScript on the current page and return the result. " +
        "Orchestrator + HIL approval required (EC-002, MAAI-001). " +
        "Returns structured result with js_result (JSON-serialized return value) and summary. " +
        "Timeout capped at EvaluationTimeoutMs (PW-LAW-003). " +
        "Use for DOM extraction, custom data scraping, or triggering page events.")]
    public async Task<BrowserResult> Evaluate(
        [Description("JavaScript expression or function body to evaluate. Must return a JSON-serializable value.")] string script,
        CancellationToken cancellationToken = default)
    {
        if (!session.IsOpen)
            return Err("No browser session open. Navigate to a page first.");

        try
        {
            object? result = null;
            await session.ExecuteAsync(async page =>
            {
                result = await page.EvaluateAsync<object>(script);
                return true;
            }, cancellationToken);

            var resultJson = JsonSerializer.Serialize(result, JsonOptions);
            var preview    = resultJson.TruncateTo(200);

            return new BrowserResult
            {
                Success    = true,
                Summary    = $"Evaluated JS — result preview: {preview}",
                Structured = new
                {
                    script_preview = script.TruncateTo(100),
                    js_result      = result,
                    result_json    = resultJson,
                    evaluated_at_utc = DateTimeOffset.UtcNow,
                },
                NextActions = ["Use the js_result in agent reasoning", "Use playwright.screenshot to capture any DOM changes"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Evaluate fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "playwright.close_session")]
    [Description(
        "TYPE 1 — Close the browser session and release all resources. " +
        "Orchestrator + HIL approval required. " +
        "Idempotent: safe to call even if session is already closed. " +
        "Always call this when the scraping workflow is complete.")]
    public async Task<BrowserResult> CloseSession(CancellationToken cancellationToken = default)
    {
        try
        {
            var wasOpen = session.IsOpen;
            await session.CloseAsync();
            return new BrowserResult
            {
                Success    = true,
                Summary    = wasOpen ? "Browser session closed successfully." : "Session was already closed — no action needed.",
                Structured = new { was_open = wasOpen, closed_at_utc = DateTimeOffset.UtcNow },
                NextActions = ["Use playwright.navigate to start a new session when needed"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Close fault: {ex.Message}");
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // TYPE 2 — Read-only inspection (no HIL required, any agent may call)
    // ════════════════════════════════════════════════════════════════════════

    [McpServerTool(Name = "playwright.get_session_status")]
    [Description(
        "TYPE 2 — Return the current browser session state. No HIL required. " +
        "Returns is_open, current_url, page_title, navigation_count, last_error. " +
        "Agent calls this before any TYPE 1 tool to verify session state without opening a browser.")]
    public BrowserResult GetSessionStatus()
    {
        var snap = session.GetStatusSnapshot();
        return new BrowserResult
        {
            Success    = true,
            Summary    = snap.IsOpen
                ? $"Session open — on '{snap.PageTitle ?? "untitled"}' ({snap.CurrentUrl ?? "no url"}), {snap.NavigationCount} navigations."
                : "Session closed — no browser running.",
            Structured = snap,
            NextActions = snap.NextActions,
        };
    }

    [McpServerTool(Name = "playwright.get_page_content")]
    [Description(
        "TYPE 2 — Extract and summarize structured content from the current page. No HIL required. " +
        "Returns: summary, headings[], links[], forms[], meta{}, text_chunks[], word_count. " +
        "This is the primary Extract+Summarize tool — the agent gets structured data without " +
        "processing raw HTML. Use this before click/fill to identify selectors and understand page structure.")]
    public async Task<BrowserResult> GetPageContent(
        [Description("If true, also return raw_html (truncated to MaxContentLengthBytes). Default: false.")] bool includeRawHtml = false,
        [Description("CSS selector to scope extraction to a specific element. Empty = full page.")] string? scopeSelector = null,
        CancellationToken cancellationToken = default)
    {
        if (!session.IsOpen)
            return Err("No browser session open. Use playwright.navigate first.");

        try
        {
            string html      = string.Empty;
            string url       = string.Empty;
            string title     = string.Empty;

            await session.ExecuteAsync(async page =>
            {
                url   = page.Url;
                title = await page.TitleAsync();
                html  = string.IsNullOrEmpty(scopeSelector)
                    ? await page.ContentAsync()
                    : await page.InnerHTMLAsync(scopeSelector);
                return true;
            }, cancellationToken);

            // ── Extract structure from HTML ──────────────────────────────────
            var headings    = ExtractHeadings(html);
            var links       = ExtractLinks(html, url);
            var forms       = ExtractForms(html);
            var meta        = ExtractMeta(html);
            var textChunks  = ExtractTextChunks(html);
            var wordCount   = textChunks.Sum(c => c.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

            // ── Truncate raw HTML if requested ───────────────────────────────
            string? rawHtml = null;
            bool htmlTruncated = false;
            if (includeRawHtml)
            {
                if (html.Length > config.MaxContentLengthBytes)
                {
                    rawHtml       = html[..config.MaxContentLengthBytes] +
                                    $"\n[HTML TRUNCATED — {html.Length:N0} chars exceeded limit of {config.MaxContentLengthBytes:N0}]";
                    htmlTruncated = true;
                }
                else
                {
                    rawHtml = html;
                }
            }

            return new BrowserResult
            {
                Success    = true,
                Summary    = $"Page '{title}' ({url}): {headings.Length} headings, {links.Length} links, {forms.Length} form(s), ~{wordCount:N0} words.",
                Structured = new
                {
                    url,
                    title,
                    scope_selector  = scopeSelector,
                    word_count      = wordCount,
                    heading_count   = headings.Length,
                    link_count      = links.Length,
                    form_count      = forms.Length,
                    headings,
                    links           = links.Take(50).ToArray(), // cap to avoid token bloat
                    forms,
                    meta,
                    text_chunks     = textChunks.Take(20).ToArray(),
                    html_truncated  = htmlTruncated,
                    raw_html        = rawHtml,
                },
                NextActions =
                [
                    "Use headings[] to understand page structure",
                    "Use links[] to find navigation targets for next playwright.navigate",
                    "Use forms[] to identify input selectors for playwright.fill",
                    "Use text_chunks[] to extract and summarize page content",
                ],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"GetPageContent fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "playwright.get_url")]
    [Description(
        "TYPE 2 — Return the current page URL and title. No HIL required. " +
        "Lightweight alternative to get_page_content when only URL/title needed.")]
    public async Task<BrowserResult> GetUrl(CancellationToken cancellationToken = default)
    {
        if (!session.IsOpen)
            return Err("No browser session open.");

        try
        {
            string url = string.Empty, title = string.Empty;
            await session.ExecuteAsync(async page =>
            {
                url   = page.Url;
                title = await page.TitleAsync();
                return true;
            }, cancellationToken);

            return new BrowserResult
            {
                Success    = true,
                Summary    = $"Current page: '{title}' — {url}",
                Structured = new { url, title, fetched_at_utc = DateTimeOffset.UtcNow },
                NextActions = ["Use playwright.get_page_content for full page extraction"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"GetUrl fault: {ex.Message}");
        }
    }

    // ── HTML extraction helpers ───────────────────────────────────────────────

    private static string[] ExtractHeadings(string html)
    {
        var matches = Regex.Matches(html, @"<h([1-6])[^>]*>(.*?)</h\1>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return matches.Select(m => $"H{m.Groups[1].Value}: {StripTags(m.Groups[2].Value).Trim()}")
                      .Where(h => !string.IsNullOrWhiteSpace(h))
                      .Take(30).ToArray();
    }

    private static object[] ExtractLinks(string html, string baseUrl)
    {
        var matches = Regex.Matches(html, @"<a\s[^>]*href=""([^""]+)""[^>]*>(.*?)</a>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return matches.Select(m =>
        {
            var href = m.Groups[1].Value.Trim();
            var text = StripTags(m.Groups[2].Value).Trim().TruncateTo(80);
            // Resolve relative URLs
            if (href.StartsWith('/') && Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                href = new Uri(baseUri, href).ToString();
            return (object)new { href, text };
        }).Where(l => ((dynamic)l).href.Length > 0).Take(100).ToArray();
    }

    private static object[] ExtractForms(string html)
    {
        var formMatches = Regex.Matches(html, @"<form[^>]*>(.*?)</form>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return formMatches.Select(fm =>
        {
            var formHtml = fm.Groups[1].Value;
            var inputs   = Regex.Matches(formHtml, @"<input[^>]*>", RegexOptions.IgnoreCase)
                .Select(im =>
                {
                    var typeM  = Regex.Match(im.Value, @"type=""([^""]+)""", RegexOptions.IgnoreCase);
                    var nameM  = Regex.Match(im.Value, @"name=""([^""]+)""", RegexOptions.IgnoreCase);
                    var idM    = Regex.Match(im.Value, @"id=""([^""]+)""", RegexOptions.IgnoreCase);
                    return new
                    {
                        type  = typeM.Success ? typeM.Groups[1].Value : "text",
                        name  = nameM.Success ? nameM.Groups[1].Value : "",
                        id    = idM.Success ? idM.Groups[1].Value : "",
                        selector = idM.Success ? $"#{idM.Groups[1].Value}"
                                 : nameM.Success ? $"input[name='{nameM.Groups[1].Value}']"
                                 : "input",
                    };
                }).ToArray();

            var actionM  = Regex.Match(fm.Value, @"action=""([^""]+)""", RegexOptions.IgnoreCase);
            var methodM  = Regex.Match(fm.Value, @"method=""([^""]+)""", RegexOptions.IgnoreCase);

            return (object)new
            {
                action = actionM.Success ? actionM.Groups[1].Value : "",
                // Fix CA1311: use ToUpper with explicit culture
                method = methodM.Success ? methodM.Groups[1].Value.ToUpper(CultureInfo.InvariantCulture) : "GET",
                inputs,
            };
        }).ToArray();
    }

    private static object ExtractMeta(string html)
    {
        // Fix: use regular string concatenation instead of interpolated raw strings
        // to avoid the $-count mismatch issue with {{ }} in regex patterns
        string GetMeta(string name)
        {
            var pattern1 = "<meta[^>]*name=\"" + name + "\"[^>]*content=\"([^\"]+)\"";
            var m = Regex.Match(html, pattern1, RegexOptions.IgnoreCase);
            if (m.Success) return m.Groups[1].Value;

            var pattern2 = "<meta[^>]*content=\"([^\"]+)\"[^>]*name=\"" + name + "\"";
            m = Regex.Match(html, pattern2, RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : "";
        }

        var canonicalMatch = Regex.Match(html, "<link[^>]*rel=\"canonical\"[^>]*href=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);

        return new
        {
            description      = GetMeta("description"),
            keywords         = GetMeta("keywords"),
            og_title         = GetMeta("og:title"),
            og_description   = GetMeta("og:description"),
            canonical        = canonicalMatch.Success ? canonicalMatch.Groups[1].Value : "",
        };
    }

    private static string[] ExtractTextChunks(string html)
    {
        // Remove scripts, styles, and tags — extract visible text paragraphs
        var noScript = Regex.Replace(html, @"<script[^>]*>.*?</script>", "",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var noStyle  = Regex.Replace(noScript, @"<style[^>]*>.*?</style>", "",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var text     = StripTags(noStyle);

        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                   .Select(l => l.Trim())
                   .Where(l => l.Length > 30)  // skip nav items and short labels
                   .Take(30)
                   .ToArray();
    }

    private static string StripTags(string html) =>
        Regex.Replace(html, @"<[^>]+>", " ").Trim();

    private static BrowserResult Err(string message) =>
        new()
        {
            Success     = false,
            Error       = message,
            Summary     = $"Error: {message}",
            NextActions = ["Read the error, self-correct parameters, and retry", "Read playwright://config for domain and timeout limits"],
        };
}

// ── Result contract ───────────────────────────────────────────────────────────

/// <summary>
/// I Am the BrowserResult. I implement the Anthropic Extract+Summarize pattern.
/// Agents read .Summary to reason, .Structured to act, .NextActions to navigate.
/// </summary>
public sealed class BrowserResult
{
    public bool Success { get; init; }

    /// <summary>One-sentence summary for direct embedding in agent reasoning chain.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Typed structured data — shape varies by tool, always addressable by field.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Structured { get; init; }

    /// <summary>Error message with self-correction guidance. Set only when Success=false.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    /// <summary>Agent-readable "what to do next" list (ANTHROPIC-AGENT-001).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? NextActions { get; init; }
}

// ── String extension ──────────────────────────────────────────────────────────
internal static class StringExtensions
{
    public static string TruncateTo(this string s, int maxLength) =>
        s.Length <= maxLength ? s : s[..maxLength] + "…";
}
