// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Services/McpToolCache.cs
// Identity   : Typed MCP tool registry — bridges HttpClient → AITool list
// Law Anchor : ARCH-NEW-001 (TYPE 1 / TYPE 2 boundary enforced here)
// ThoughtLock: 2026-05-30
//
// All MCP calls route through CallMcp() — a static JSON-RPC 2.0 POST wrapper.
// TYPE 2 tools are read-only and may be given to any phase agent.
// TYPE 1 tools (WriteFile, RunCommand, NavigateTo) are listed here so agents
// know they exist, but ToolFilterChatClient blocks them for non-Maker agents.
// Only the Maker receives AgentToolSet.FullMaker which includes TYPE 1 tools.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;
using ProjectName.AgentService.Infrastructure;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectName.AgentService.Services;

public sealed class McpToolCache(
    IHttpClientFactory httpClientFactory,
    ILogger<McpToolCache> logger)
{
    private readonly HttpClient _fsClient   = httpClientFactory.CreateClient("mcp-filesystem");
    private readonly HttpClient _termClient = httpClientFactory.CreateClient("mcp-terminal");
    private readonly HttpClient _pwClient   = httpClientFactory.CreateClient("mcp-playwright");

    // ── Full tool list ────────────────────────────────────────────────────────
    // AIFunction names (LLM-facing) are short and friendly.
    // CallMcp name strings inside each method match the actual MCP server.
    public List<AITool> GetNativeTools() =>
    [
        // ── Filesystem MCP — TYPE 2 (read-only) ──────────────────────────────
        AIFunctionFactory.Create(ReadFile,    "ReadFile",    "Read a file by path. TYPE 2."),
        AIFunctionFactory.Create(ListDir,     "ListDir",     "List files and directories at a path. TYPE 2."),
        AIFunctionFactory.Create(FileExists,  "FileExists",  "Check whether a file or directory exists. TYPE 2."),
        AIFunctionFactory.Create(GetInfo,     "GetInfo",     "Get size, line count, and metadata for a file. TYPE 2."),

        // ── Filesystem MCP — TYPE 1 (world-changing) ─────────────────────────
        AIFunctionFactory.Create(WriteFile,   "WriteFile",   "Create or overwrite a file. TYPE 1 — Orchestrator dispatches after HIL."),
        AIFunctionFactory.Create(DeleteFile,  "DeleteFile",  "Delete a file or directory. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(MoveFile,    "MoveFile",    "Move or rename a file. TYPE 1 — HIL required."),

        // ── Terminal MCP — TYPE 2 (read-only) ────────────────────────────────
        AIFunctionFactory.Create(Which,             "Which",             "Check if a binary exists on PATH. TYPE 2."),
        AIFunctionFactory.Create(GetTerminalStatus, "GetTerminalStatus", "Return idle/running status for all terminal slots. TYPE 2."),
        AIFunctionFactory.Create(GetEnvironment,    "GetEnvironment",    "Return environment variables visible to the terminal. TYPE 2."),

        // ── Terminal MCP — TYPE 1 (world-changing) ───────────────────────────
        AIFunctionFactory.Create(RunCommand,  "RunCommand",  "Execute a shell command. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(RunScript,   "RunScript",   "Write and execute a multi-line script. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(KillProcess, "KillProcess", "Kill all processes in a terminal slot. TYPE 1 — HIL required."),

        // ── Playwright MCP — TYPE 2 (read-only) ──────────────────────────────
        AIFunctionFactory.Create(GetSessionStatus, "GetSessionStatus", "Return browser session state. TYPE 2."),
        AIFunctionFactory.Create(GetPageContent,   "GetPageContent",   "Extract structured content from the current page. TYPE 2."),
        AIFunctionFactory.Create(GetPageUrl,       "GetPageUrl",       "Return the current page URL and title. TYPE 2."),

        // ── Playwright MCP — TYPE 1 (network-touching / world-changing) ──────
        AIFunctionFactory.Create(Navigate,      "Navigate",      "Navigate browser to a URL. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(BrowserClick,   "BrowserClick",  "Click an element on the current page. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(BrowserFill,    "BrowserFill",   "Fill a text input on the current page. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(TakeScreenshot, "TakeScreenshot","Capture a PNG screenshot. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(EvaluateJs,     "EvaluateJs",    "Execute JavaScript on the current page. TYPE 1 — HIL required."),
        AIFunctionFactory.Create(CloseSession,   "CloseSession",  "Close the browser session. TYPE 1 — HIL required."),
    ];

    // ── Filesystem — TYPE 2 ───────────────────────────────────────────────────
    [Description("Read a file by path. Use fromLine/toLine for large files.")]
    private Task<string> ReadFile(
        [Description("Absolute path under an AllowedRoot")] string path,
        [Description("First line to return (1-based, optional)")] int? fromLine = null,
        [Description("Last line to return (1-based, optional)")] int? toLine = null)
        => CallMcp(_fsClient, "filesystem.read_file", new { path, fromLine, toLine });

    [Description("List files and directories at a path.")]
    private Task<string> ListDir(
        [Description("Absolute directory path")] string path,
        [Description("Glob filter, empty = all")] string pattern = "",
        [Description("Recurse into subdirectories")] bool recursive = false)
        => CallMcp(_fsClient, "filesystem.list_directory", new { path, pattern, recursive });

    [Description("Check whether a file or directory exists.")]
    private Task<string> FileExists([Description("Absolute path")] string path)
        => CallMcp(_fsClient, "filesystem.file_exists", new { path });

    [Description("Get size, line count, and metadata for a file.")]
    private Task<string> GetInfo([Description("Absolute path")] string path)
        => CallMcp(_fsClient, "filesystem.get_info", new { path });

    // ── Filesystem — TYPE 1 ───────────────────────────────────────────────────
    [Description("Create or overwrite a file. TYPE 1.")]
    private Task<string> WriteFile(
        [Description("Absolute path")] string path,
        [Description("Full content to write")] string content,
        [Description("Encoding: utf-8 (default) | utf-8-bom | ascii")] string encoding = "utf-8")
        => CallMcp(_fsClient, "filesystem.write_file", new { path, content, encoding });

    [Description("Delete a file or directory. TYPE 1.")]
    private Task<string> DeleteFile(
        [Description("Absolute path")] string path,
        [Description("Delete directory recursively")] bool recursive = false)
        => CallMcp(_fsClient, "filesystem.delete_file", new { path, recursive });

    [Description("Move or rename a file. TYPE 1.")]
    private Task<string> MoveFile(
        [Description("Source absolute path")] string sourcePath,
        [Description("Destination absolute path")] string destinationPath,
        [Description("Overwrite destination if it exists")] bool overwrite = false)
        => CallMcp(_fsClient, "filesystem.move_file", new { sourcePath, destinationPath, overwrite });

    // ── Terminal — TYPE 2 ─────────────────────────────────────────────────────
    [Description("Check if a command exists on PATH.")]
    private Task<string> Which([Description("Command name, e.g. 'dotnet', 'git'")] string command)
        => CallMcp(_termClient, "terminal.which", new { command });

    [Description("Return idle/running status for all terminal slots.")]
    private Task<string> GetTerminalStatus()
        => CallMcp(_termClient, "terminal.get_status", new { });

    [Description("Return environment variables visible to the terminal.")]
    private Task<string> GetEnvironment(
        [Description("Variable names to return. Empty = standard PMCR-O set.")] string[] variables)
        => CallMcp(_termClient, "terminal.get_environment", new { variables });

    // ── Terminal — TYPE 1 ─────────────────────────────────────────────────────
    [Description("Execute a shell command in a named slot. TYPE 1.")]
    private Task<string> RunCommand(
        [Description("Slot: terminal-1 (general) | terminal-2 (git) | terminal-3 (packages) | terminal-4 (long-running)")] string slot,
        [Description("Shell command. Single command — do not chain with &&.")] string command,
        [Description("Working dir relative to WorkingRoot. Empty = WorkingRoot.")] string workingDir = "")
        => CallMcp(_termClient, "terminal.run_command", new { slot, command, workingDir });

    [Description("Write and execute a multi-line script. TYPE 1.")]
    private Task<string> RunScript(
        [Description("Slot: terminal-1 | terminal-2 | terminal-3 | terminal-4")] string slot,
        [Description("Full script content")] string scriptContent,
        [Description("Extension: .ps1 | .sh | .py | .cmd")] string extension = ".sh",
        [Description("Working dir relative to WorkingRoot")] string workingDir = "")
        => CallMcp(_termClient, "terminal.run_script", new { slot, scriptContent, extension, workingDir });

    [Description("Kill all processes in a terminal slot. TYPE 1.")]
    private Task<string> KillProcess(
        [Description("Slot: terminal-1 | terminal-2 | terminal-3 | terminal-4")] string slot)
        => CallMcp(_termClient, "terminal.kill", new { slot });

    // ── Playwright — TYPE 2 ───────────────────────────────────────────────────
    [Description("Return the current browser session state.")]
    private Task<string> GetSessionStatus()
        => CallMcp(_pwClient, "playwright.get_session_status", new { });

    [Description("Extract structured content from the current page: headings, links, forms, text.")]
    private Task<string> GetPageContent(
        [Description("Include raw HTML in the response")] bool includeRawHtml = false,
        [Description("CSS selector to scope extraction. Empty = full page.")] string? scopeSelector = null)
        => CallMcp(_pwClient, "playwright.get_page_content", new { includeRawHtml, scopeSelector });

    [Description("Return the current page URL and title.")]
    private Task<string> GetPageUrl()
        => CallMcp(_pwClient, "playwright.get_url", new { });

    // ── Playwright — TYPE 1 ───────────────────────────────────────────────────
    [Description("Navigate browser to a URL. TYPE 1.")]
    private Task<string> Navigate(
        [Description("Absolute URL (http:// or https://)")] string url,
        [Description("Wait strategy: domcontentloaded (default) | load | networkidle")] string waitUntil = "domcontentloaded")
        => CallMcp(_pwClient, "playwright.navigate", new { url, waitUntil });

    [Description("Click an element on the current page. TYPE 1.")]
    private Task<string> BrowserClick(
        [Description("CSS selector or text selector, e.g. 'button[type=submit]' or 'text=Login'")] string selector,
        [Description("Wait for navigation to complete after click")] bool waitForNavigation = false)
        => CallMcp(_pwClient, "playwright.click", new { selector, waitForNavigation });

    [Description("Fill a text input on the current page. TYPE 1.")]
    private Task<string> BrowserFill(
        [Description("CSS selector for the input, e.g. 'input[name=email]'")] string selector,
        [Description("Value to fill")] string value,
        [Description("Press Enter after filling")] bool pressEnter = false)
        => CallMcp(_pwClient, "playwright.fill", new { selector, value, pressEnter });

    [Description("Capture a PNG screenshot of the current page. TYPE 1.")]
    private Task<string> TakeScreenshot(
        [Description("Capture full scrollable page height")] bool fullPage = false,
        [Description("CSS selector to capture only that element")] string? elementSelector = null)
        => CallMcp(_pwClient, "playwright.screenshot", new { fullPage, elementSelector });

    [Description("Execute JavaScript on the current page and return the result. TYPE 1.")]
    private Task<string> EvaluateJs(
        [Description("JavaScript expression to evaluate. Must return a JSON-serializable value.")] string script)
        => CallMcp(_pwClient, "playwright.evaluate", new { script });

    [Description("Close the browser session. TYPE 1.")]
    private Task<string> CloseSession()
        => CallMcp(_pwClient, "playwright.close_session", new { });

    // ── JSON-RPC caller — static: accesses no instance data ──────────────────
    private static async Task<string> CallMcp(HttpClient client, string tool, object args)
    {
        try
        {
            var request = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = "tools/call",
                @params = new { name = tool, arguments = args }
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var resp = await client.PostAsJsonAsync("/mcp", request, cts.Token);
            resp.EnsureSuccessStatusCode();

            var rawJson = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(rawJson);

            if (doc.RootElement.TryGetProperty("result", out var result) &&
                result.TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.Array)
            {
                foreach (var block in content.EnumerateArray())
                {
                    if (block.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                        block.TryGetProperty("text", out var text))
                    {
                        var textValue = text.GetString() ?? string.Empty;

                        // MCP servers return augmented JSON (FileResult, TerminalResult, BrowserResult).
                        // qwen3:8b can't reliably parse these to determine success/failure.
                        // Extract the Summary field (human-readable) so the model gets a clean
                        // confirmation it can act on without re-reading the full JSON blob.
                        try
                        {
                            using var inner = JsonDocument.Parse(textValue);
                            var root = inner.RootElement;

                            // FileResult / BrowserResult — has Summary field
                            if (root.TryGetProperty("Summary", out var summary) ||
                                root.TryGetProperty("summary", out summary))
                            {
                                var summaryText = summary.GetString() ?? textValue;

                                // Prepend ERROR: if success=false so model knows to retry
                                if ((root.TryGetProperty("Success", out var success) ||
                                     root.TryGetProperty("success", out success)) &&
                                    success.ValueKind == JsonValueKind.False)
                                    return $"ERROR: {summaryText}";

                                // Success — append an explicit stop cue so qwen3:8b produces a
                                // final text response instead of calling another tool.
                                return $"{summaryText} [Done. Reply to the user now.]";
                            }

                            // TerminalResult — has Stdout/Stderr/ExitCode
                            if (root.TryGetProperty("Stdout", out var stdout) ||
                                root.TryGetProperty("stdout", out stdout))
                            {
                                var stdoutText = stdout.GetString() ?? string.Empty;
                                var exitCode   = root.TryGetProperty("ExitCode", out var ec)
                                    ? ec.ToString() : "?";
                                var stderr = root.TryGetProperty("Stderr", out var se)
                                    ? se.GetString() : null;

                                var output = $"ExitCode={exitCode}";
                                if (!string.IsNullOrWhiteSpace(stdoutText)) output += $"\nstdout:\n{stdoutText}";
                                if (!string.IsNullOrWhiteSpace(stderr))     output += $"\nstderr:\n{stderr}";
                                return output;
                            }
                        }
                        catch (JsonException)
                        {
                            // Not JSON or not an augmented result — return raw text as-is
                        }

                        return textValue;
                    }
                }
            }

            if (doc.RootElement.TryGetProperty("error", out var error))
                return $"MCP error: {error.GetRawText()}";

            return rawJson;
        }
        catch (Exception ex)
        {
            return $"MCP call failed [{tool}]: {ex.Message}";
        }
    }

    // ── Startup probe ─────────────────────────────────────────────────────────
    public async Task ProbeAsync()
    {
        foreach (var (name, client) in new (string, HttpClient)[]
        {
            ("filesystem", _fsClient),
            ("terminal",   _termClient),
            ("playwright", _pwClient),
        })
        {
            try
            {
                var req = new { jsonrpc = "2.0", id = "probe", method = "tools/list", @params = new { } };
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var r = await client.PostAsJsonAsync("/mcp", req, cts.Token);
                // CA1873: pass status as a pre-evaluated string, not inside the log template eval path.
                var status = r.IsSuccessStatusCode ? "OK" : $"HTTP {(int)r.StatusCode}";
                McpProbeLog.ProbeOk(logger, name, status);
            }
            catch (Exception ex)
            {
                McpProbeLog.ProbeUnreachable(logger, name, ex.Message);
            }
        }
    }
}
