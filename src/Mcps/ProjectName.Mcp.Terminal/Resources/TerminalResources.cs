// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.TERMINAL
// File       : Resources/TerminalResources.cs
// Identity   : MCP Pillar 2 — Resources (read-only contextual data for Agents)
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, EC-005, PRODUCT-002, ANTHROPIC-ACI-001
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using System.Collections.Concurrent;
using System.Text.Json;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using ProjectName.Mcp.Terminal.Configuration;
using ProjectName.Mcp.Terminal.Tools;
using ModelContextProtocol;

namespace ProjectName.Mcp.Terminal.Resources;

/// <summary>
/// I Am the Terminal MCP Resource Provider. I am the "Memory" layer of the
/// ProjectName.Mcp.Terminal server — Pillar 2 of the three MCP primitives.
/// I expose read-only contextual data so Agents can understand the terminal
/// execution environment before issuing commands. I am TYPE 2 — no HIL required.
/// Any phase agent may read me at any time (EC-002).
/// </summary>
[McpServerResourceType]
public sealed class TerminalResources(TerminalConfig config, ILogger<TerminalResources> logger)
{
    // Shared execution history — written by TerminalTools after every RunCommand,
    // read by Agents via terminal://history/{slot}.
    internal static readonly ConcurrentDictionary<string, TerminalResult> SlotHistory = new();

    private static readonly HashSet<string> ValidSlots =
        ["terminal-1", "terminal-2", "terminal-3", "terminal-4"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ── LoggerMessage delegates (CA1848) ─────────────────────────────────────
    private static readonly Action<ILogger, Exception?> _logStatus =
        LoggerMessage.Define(LogLevel.Debug, new EventId(10, "ResStatus"), "[RES] terminal://status fetched");

    private static readonly Action<ILogger, Exception?> _logEnv =
        LoggerMessage.Define(LogLevel.Debug, new EventId(11, "ResEnv"), "[RES] terminal://environment fetched");

    private static readonly Action<ILogger, Exception?> _logConfig =
        LoggerMessage.Define(LogLevel.Debug, new EventId(12, "ResConfig"), "[RES] terminal://config fetched");

    private static readonly Action<ILogger, Exception?> _logSkill =
        LoggerMessage.Define(LogLevel.Debug, new EventId(13, "ResSkill"), "[RES] terminal://skill fetched");

    private static readonly Action<ILogger, string, Exception?> _logHistory =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(14, "ResHistory"), "[RES] terminal://history/{Slot} fetched");

    private static readonly Action<ILogger, string, Exception?> _logSkillFallback =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(15, "SkillFallback"),
            "[RES] terminal://skill — SKILL.md not found at {Path}, returning inline fallback");

    // ════════════════════════════════════════════════════════════════════════
    // Direct Resources — fixed URI, always in resources/list
    // ════════════════════════════════════════════════════════════════════════

    [McpServerResource(
        UriTemplate = "terminal://status",
        Name        = "Terminal Slot Status",
        MimeType    = "application/json")]
    [Description(
        "TYPE 2 — Returns the idle/running state of all four terminal slots as JSON. " +
        "Planner reads this before writing RunCommand steps — do not plan against a running slot.")]
    public TextResourceContents GetStatus()
    {
        _logStatus(logger, null);

        var status = new
        {
            thoughtlock = "2026-05-30",
            slots = new Dictionary<string, object>
            {
                ["terminal-1"] = new
                {
                    state   = IsRunning("terminal-1") ? "running" : "idle",
                    purpose = "General: build, test, dotnet commands",
                    timeout = $"{config.GetTimeoutSeconds("terminal-1")}s",
                },
                ["terminal-2"] = new
                {
                    state   = IsRunning("terminal-2") ? "running" : "idle",
                    purpose = "Git operations",
                    timeout = $"{config.GetTimeoutSeconds("terminal-2")}s",
                },
                ["terminal-3"] = new
                {
                    state   = IsRunning("terminal-3") ? "running" : "idle",
                    purpose = "Package managers: dotnet add, npm, pip",
                    timeout = $"{config.GetTimeoutSeconds("terminal-3")}s",
                },
                ["terminal-4"] = new
                {
                    state   = IsRunning("terminal-4") ? "running" : "idle",
                    purpose = "Long-running: Playwright, scrapers (15 min timeout)",
                    timeout = $"{config.GetTimeoutSeconds("terminal-4")}s",
                },
            },
            working_root = config.WorkingRoot,
            type_boundary = new
            {
                type1_tools = new[] { "terminal.run_command", "terminal.run_script", "terminal.kill" },
                type2_tools = new[] { "terminal.get_status", "terminal.get_environment", "terminal.which" },
                note = "TYPE 1 tools require Orchestrator + HIL approval (EC-002, MAAI-001). Resources are always TYPE 2.",
            },
        };

        return new TextResourceContents
        {
            Uri      = "terminal://status",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(status, JsonOptions),
        };
    }

    [McpServerResource(
        UriTemplate = "terminal://environment",
        Name        = "Terminal Environment",
        MimeType    = "application/json")]
    [Description(
        "TYPE 2 — Returns environment variables visible to the terminal server process. " +
        "Planner reads this before writing commands that depend on external tools.")]
    public TextResourceContents GetEnvironment()
    {
        _logEnv(logger, null);

        var vars = new[]
        {
            "PATH", "HOME", "USERPROFILE", "TEMP", "TMP",
            "DOTNET_ROOT", "DOTNET_VERSION",
            "NODE_PATH", "NODE_VERSION",
            "PYTHON_PATH", "PYTHONPATH",
            "ASPNETCORE_ENVIRONMENT",
            "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL",
            "OTEL_EXPORTER_OTLP_ENDPOINT",
            "GIT_EXEC_PATH",
            "NuGetPackageRoot",
        };

        var env = new Dictionary<string, string>(
            vars.Select(v => KeyValuePair.Create(v, Environment.GetEnvironmentVariable(v) ?? "(not set)")));

        var payload = new
        {
            thoughtlock        = "2026-05-30",
            environment        = env,
            working_root       = config.WorkingRoot,
            shell              = config.Shell,
            shell_command_flag = config.ShellCommandFlag,
            note = "If a tool root shows '(not set)', verify with terminal.which before planning commands that use that tool.",
        };

        return new TextResourceContents
        {
            Uri      = "terminal://environment",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(payload, JsonOptions),
        };
    }

    [McpServerResource(
        UriTemplate = "terminal://config",
        Name        = "Terminal Server Config",
        MimeType    = "application/json")]
    [Description(
        "TYPE 2 — Returns the terminal server configuration: WorkingRoot (sandbox boundary), " +
        "per-slot timeouts, MaxOutputBytes (truncation threshold), and shell settings.")]
    public TextResourceContents GetConfig()
    {
        _logConfig(logger, null);

        var payload = new
        {
            thoughtlock             = "2026-05-30",
            working_root            = config.WorkingRoot,
            default_timeout_seconds = config.CommandTimeoutSeconds,
            max_output_bytes        = config.MaxOutputBytes,
            shell                   = config.Shell,
            shell_command_flag      = config.ShellCommandFlag,
            slot_timeout_overrides  = config.SlotTimeouts,
            safety_note             = "SAFETY-003: No command may execute outside WorkingRoot.",
            law_anchors             = new[] { "EC-002", "MAAI-001", "SAFETY-003", "FRAC-MCP-400-001" },
        };

        return new TextResourceContents
        {
            Uri      = "terminal://config",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(payload, JsonOptions),
        };
    }

    [McpServerResource(
        UriTemplate = "terminal://skill",
        Name        = "Terminal MCP SKILL.md",
        MimeType    = "text/markdown")]
    [Description(
        "TYPE 2 — Returns the SKILL.md capability manifest for this Terminal MCP server. " +
        "Any agent reads this before issuing the first terminal command in a session.")]
    public TextResourceContents GetSkill()
    {
        _logSkill(logger, null);

        var assemblyDir = Path.GetDirectoryName(typeof(TerminalResources).Assembly.Location)
                          ?? AppContext.BaseDirectory;
        var skillPath   = Path.Combine(assemblyDir, "skills", "terminal-mcp", "SKILL.md");

        string skillContent;
        if (File.Exists(skillPath))
        {
            skillContent = File.ReadAllText(skillPath);
        }
        else
        {
            skillContent = GetInlineSkillFallback();
            _logSkillFallback(logger, skillPath, null);
        }

        return new TextResourceContents
        {
            Uri      = "terminal://skill",
            MimeType = "text/markdown",
            Text     = skillContent,
        };
    }

    // ════════════════════════════════════════════════════════════════════════
    // Templated Resources — URI template, appear in resources/templates/list
    // ════════════════════════════════════════════════════════════════════════

    [McpServerResource(
        UriTemplate = "terminal://history/{slot}",
        Name        = "Terminal Slot History",
        MimeType    = "application/json")]
    [Description(
        "TYPE 2 — Returns the last TerminalResult for the named slot. " +
        "Checker reads this after RunCommand dispatch to score the result without re-running. " +
        "Valid slots: terminal-1, terminal-2, terminal-3, terminal-4.")]
    public TextResourceContents GetHistory(
        [Description("Slot name: terminal-1 | terminal-2 | terminal-3 | terminal-4")] string slot)
    {
        _logHistory(logger, slot, null);

        if (!ValidSlots.Contains(slot))
            throw new McpException($"Invalid slot '{slot}'. Valid: {string.Join(", ", ValidSlots)}");

        var result = SlotHistory.TryGetValue(slot, out var history)
            ? history
            : new TerminalResult
            {
                Success = true,
                Slot    = slot,
                Stdout  = $"No command history for '{slot}' in this session.",
            };

        var payload = new
        {
            thoughtlock  = "2026-05-30",
            slot,
            has_history  = SlotHistory.ContainsKey(slot),
            last_result  = result,
            type_note    = "Read this resource to score RunCommand results without re-running (Checker pattern, TYPE 2).",
        };

        return new TextResourceContents
        {
            Uri      = $"terminal://history/{slot}",
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(payload, JsonOptions),
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool IsRunning(string slot) =>
        TerminalTools.ActiveProcesses.Keys.Any(k => k.StartsWith($"{slot}:", StringComparison.Ordinal));

    private static string GetInlineSkillFallback() =>
        """
        ---
        name: terminal-mcp
        tier: SHARED — Pillar 3 Infrastructure
        type_boundary:
          type1: [terminal.run_command, terminal.run_script, terminal.kill]
          type2: [terminal.get_status, terminal.get_environment, terminal.which]
        resources:
          - terminal://status
          - terminal://environment
          - terminal://config
          - terminal://skill
          - terminal://history/{slot}
        prompts:
          - terminal-run-command
          - terminal-debug-failure
          - terminal-plan-commands
        slots:
          terminal-1: General (build, test, dotnet) — 60s timeout
          terminal-2: Git operations — 60s timeout
          terminal-3: Package managers (dotnet add, npm, pip) — 60s timeout
          terminal-4: Long-running / Playwright — 900s timeout
        law_anchors: [EC-002, MAAI-001, SAFETY-003, FRAC-MCP-400-001, EC-005]
        thoughtlock: "2026-05-30"
        ---
        Full SKILL.md not found on disk. Deploy skills/terminal-mcp/SKILL.md adjacent to the assembly.
        """;
}
