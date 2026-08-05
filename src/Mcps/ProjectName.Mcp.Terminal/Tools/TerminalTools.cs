// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.TERMINAL
// File       : Tools/TerminalTools.cs
// Identity   : Four-slot Terminal Actuator — the "Hands" of the cognitive stack
// Pillar     : 3 — Infrastructure (MCP Server, not an Agent)
// Law Anchor : EC-002, MAAI-001, SAFETY-003, ANTHROPIC-ACI-001
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using ProjectName.Mcp.Terminal.Configuration;

// Explicit alias — ImplicitUsings + MCP SDK both resolve Description; pin to SCM.
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ProjectName.Mcp.Terminal.Tools;

/// <summary>
/// I Am the Terminal MCP Tool Provider. I am the "Hands" of the PMCR-O cognitive
/// stack. I expose a four-slot shell execution surface that Agents use to interact
/// with the operating environment. I do not reason. I do not plan. I execute.
/// </summary>
[McpServerToolType]
public sealed class TerminalTools(TerminalConfig config, ILogger<TerminalTools> logger)
{
    // Tracks all in-flight processes keyed by "{slot}:{guid}" for KillProcess targeting.
    // Internal — exposed to TerminalResources for slot-state reads (TYPE 2, no HIL).
    internal static readonly ConcurrentDictionary<string, Process> ActiveProcesses = new();

    private static readonly HashSet<string> ValidSlots =
        ["terminal-1", "terminal-2", "terminal-3", "terminal-4"];

    // ── LoggerMessage delegates (CA1848 / CA1873 — performance) ─────────────
    private static readonly Action<ILogger, string, string, string, Exception?> _logRunCommand =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Information, new EventId(1, "RunCommand"),
            "[TERM:{Slot}] RunCommand: {Cmd} | dir={Dir}");

    private static readonly Action<ILogger, string, int, Exception?> _logExitCode =
        LoggerMessage.Define<string, int>(
            LogLevel.Information, new EventId(2, "ExitCode"),
            "[TERM:{Slot}] ExitCode={Code}");

    private static readonly Action<ILogger, string, int, string, Exception?> _logTimeout =
        LoggerMessage.Define<string, int, string>(
            LogLevel.Warning, new EventId(3, "Timeout"),
            "[TERM:{Slot}] Timeout after {Sec}s: {Cmd}");

    private static readonly Action<ILogger, string, Exception?> _logFault =
        LoggerMessage.Define<string>(
            LogLevel.Error, new EventId(4, "Fault"),
            "[TERM:{Slot}] RunCommand fault");

    private static readonly Action<ILogger, string, string, string, Exception?> _logRunScript =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Information, new EventId(5, "RunScript"),
            "[TERM:{Slot}] RunScript ext={Ext} tmpFile={File}");

    private static readonly Action<ILogger, string, int, Exception?> _logKilled =
        LoggerMessage.Define<string, int>(
            LogLevel.Information, new EventId(6, "Killed"),
            "[TERM:{Slot}] Killed {N} process(es)");

    private static readonly Action<ILogger, string, Exception?> _logKillFail =
        LoggerMessage.Define<string>(
            LogLevel.Warning, new EventId(7, "KillFail"),
            "[TERM:{Slot}] KillProcess partial failure");

    // ════════════════════════════════════════════════════════════════════════
    // TYPE 1 — World-changing tools
    // Orchestrator + HIL approval (MAAI-001) required before dispatch.
    // ════════════════════════════════════════════════════════════════════════

    [McpServerTool(Name = "terminal.run_command")]
    [Description(
       "TYPE 1 — Execute a single shell command in the named terminal slot. " +
       "Orchestrator + HIL approval required (EC-002, MAAI-001). " +
       "Slots: terminal-1 (general/build), terminal-2 (git), " +
       "terminal-3 (packages: dotnet add / npm / pip), terminal-4 (long-running/Playwright). " +
       "Working directory is sandboxed to WorkingRoot — relative paths only. " +
       "Returns structured TerminalResult with ExitCode, Stdout, Stderr.")]
    public async Task<TerminalResult> RunCommand(
        [Description("Slot name: terminal-1 | terminal-2 | terminal-3 | terminal-4")] string slot,
        [Description("Shell command to execute. Single command only — do not chain with &&.")] string command,
        [Description("Working directory RELATIVE to WorkingRoot. Empty = WorkingRoot. Must not start with '..'.")] string workingDir = "",
        [Description("Additional environment variables as KEY=VALUE strings.")] string[]? env = null,
        CancellationToken cancellationToken = default)
    {
        if (!ValidSlots.Contains(slot))
            return Err($"Invalid slot '{slot}'. Valid slots: {string.Join(", ", ValidSlots)}");

        var resolvedDir = ResolveWorkingDir(workingDir);
        if (resolvedDir is null)
            return Err(
                $"Working directory '{workingDir}' escapes the sandbox root '{config.WorkingRoot}'. " +
                "Only paths within WorkingRoot are permitted (SAFETY-003).");

        _logRunCommand(logger, slot, command, resolvedDir, null);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(config.GetTimeoutSeconds(slot)));

        var psi = BuildProcessStartInfo(command, resolvedDir, env);

        try
        {
            using var process = new Process { StartInfo = psi };
            var stdout      = new StringBuilder();
            var stderr      = new StringBuilder();
            var outputBytes = 0;

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                outputBytes += Encoding.UTF8.GetByteCount(e.Data);
                if (outputBytes < config.MaxOutputBytes)
                    stdout.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null) stderr.AppendLine(e.Data);
            };

            var processKey = $"{slot}:{Guid.NewGuid():N}";
            process.Start();
            ActiveProcesses[processKey] = process;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cts.Token);
            ActiveProcesses.TryRemove(processKey, out _);

            var stdoutStr = stdout.ToString();
            if (outputBytes >= config.MaxOutputBytes)
                stdoutStr += $"\n[OUTPUT TRUNCATED — {outputBytes} bytes exceeded limit of {config.MaxOutputBytes}]";

            _logExitCode(logger, slot, process.ExitCode, null);

            var result = new TerminalResult
            {
                Success    = process.ExitCode == 0,
                Slot       = slot,
                Command    = command,
                ExitCode   = process.ExitCode,
                Stdout     = stdoutStr.TrimEnd(),
                Stderr     = stderr.ToString().TrimEnd(),
                WorkingDir = resolvedDir,
                ExecutedAt = DateTimeOffset.UtcNow,
            };

            // Write to history — Checker/Reflector read terminal://history/{slot}
            Resources.TerminalResources.SlotHistory[slot] = result;

            return result;
        }
        catch (OperationCanceledException)
        {
            _logTimeout(logger, slot, config.GetTimeoutSeconds(slot), command, null);
            return Err(
                $"Command timed out after {config.GetTimeoutSeconds(slot)}s. " +
                $"Use KillProcess('{slot}') to ensure cleanup, then retry or escalate.");
        }
        catch (Exception ex)
        {
            _logFault(logger, slot, ex);
            return Err($"Shell invocation fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "terminal.run_script")]
    [Description(
       "TYPE 1 — Write a multi-line script to a temp file and execute it in the named slot. " +
       "Orchestrator + HIL approval required (EC-002, MAAI-001). " +
       "Supported extensions: .ps1 (PowerShell), .sh (bash), .py (Python), .cmd (batch). " +
       "Temp file is always cleaned up after execution.")]
    public async Task<TerminalResult> RunScript(
        [Description("Slot name: terminal-1 | terminal-2 | terminal-3 | terminal-4")] string slot,
        [Description("Full script content. Write complete, self-contained scripts.")] string scriptContent,
        [Description("File extension that selects the interpreter: .ps1 | .sh | .py | .cmd")] string extension = ".sh",
        [Description("Working directory relative to WorkingRoot. Empty = WorkingRoot.")] string workingDir = "",
        CancellationToken cancellationToken = default)
    {
        if (!ValidSlots.Contains(slot))
            return Err($"Invalid slot '{slot}'. Valid slots: {string.Join(", ", ValidSlots)}");

        var tmpFile = Path.Combine(Path.GetTempPath(), $"pmcro-{slot}-{Guid.NewGuid():N}{extension}");

        try
        {
            await File.WriteAllTextAsync(tmpFile, scriptContent, cancellationToken);

            var invocation = extension switch
            {
                ".ps1" => $"powershell.exe -NonInteractive -ExecutionPolicy Bypass -File \"{tmpFile}\"",
                ".py"  => $"python \"{tmpFile}\"",
                ".sh"  => $"/bin/bash \"{tmpFile}\"",
                _      => $"{config.Shell} {config.ShellCommandFlag} \"{tmpFile}\"",
            };

            _logRunScript(logger, slot, extension, tmpFile, null);
            return await RunCommand(slot, invocation, workingDir, null, cancellationToken);
        }
        finally
        {
            try { File.Delete(tmpFile); } catch { /* best-effort */ }
        }
    }

    [McpServerTool(Name = "terminal.kill")]
    [Description(
    "TYPE 1 — Kill all running processes in the named slot, including child process trees. " +
    "Orchestrator + HIL approval required (EC-002, MAAI-001). " +
    "Idempotent: returns success if no process is running in the slot.")]
    public TerminalResult KillProcess(
        [Description("Slot name: terminal-1 | terminal-2 | terminal-3 | terminal-4")] string slot)
    {
        if (!ValidSlots.Contains(slot))
            return Err($"Invalid slot '{slot}'.");

        var keys = ActiveProcesses.Keys
            .Where(k => k.StartsWith($"{slot}:", StringComparison.Ordinal))
            .ToList();

        if (keys.Count == 0)
            return new TerminalResult
            {
                Success = true,
                Slot    = slot,
                Stdout  = $"No active process in '{slot}' — slot is idle.",
            };

        var killed = 0;
        foreach (var key in keys)
        {
            if (ActiveProcesses.TryRemove(key, out var proc))
            {
                try { proc.Kill(entireProcessTree: true); killed++; }
                catch (Exception ex) { _logKillFail(logger, slot, ex); }
            }
        }

        _logKilled(logger, slot, killed, null);
        return new TerminalResult
        {
            Success  = true,
            Slot     = slot,
            Stdout   = $"Killed {killed} process(es) in '{slot}'.",
            ExitCode = -1,
        };
    }

    // ════════════════════════════════════════════════════════════════════════
    // TYPE 2 — Read-only inspection tools
    // Any phase agent may call these directly — no HIL required (EC-002).
    // ════════════════════════════════════════════════════════════════════════

    [McpServerTool(Name = "terminal.get_status")]
    [Description(
         "TYPE 2 — Return idle/running status for all four terminal slots. " +
         "Any phase agent may call (no HIL required). " +
         "Planner calls this before proposing RunCommand steps to verify slot availability.")]
    public TerminalResult GetTerminalStatus()
    {
        var statuses = ValidSlots.ToDictionary(
            slot => slot,
            slot => ActiveProcesses.Keys.Any(k => k.StartsWith($"{slot}:", StringComparison.Ordinal))
                ? "running"
                : "idle");

        return new TerminalResult
        {
            Success = true,
            Stdout  = JsonSerializer.Serialize(statuses, JsonOptions),
        };
    }

    [McpServerTool(Name = "terminal.get_environment")]
    [Description(
       "TYPE 2 — Return environment variables visible to the terminal server. " +
       "Any phase agent may call (no HIL required). " +
       "Planner calls this to verify tool availability before writing RunCommand steps.")]
    public TerminalResult GetEnvironment(
        [Description("Variable names to return. Empty = standard PMCR-O environment set.")] string[] variables)
    {
        var standardSet = new[]
        {
            "PATH", "HOME", "USERPROFILE", "TEMP", "TMP",
            "DOTNET_ROOT", "DOTNET_VERSION",
            "NODE_PATH", "NODE_VERSION",
            "PYTHON_PATH", "PYTHONPATH",
            "ASPNETCORE_ENVIRONMENT",
            "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL",
            "OTEL_EXPORTER_OTLP_ENDPOINT",
        };

        var targets = variables.Length > 0 ? variables : standardSet;
        var result  = targets.ToDictionary(
            v => v,
            v => Environment.GetEnvironmentVariable(v) ?? "(not set)");

        return new TerminalResult
        {
            Success = true,
            Stdout  = JsonSerializer.Serialize(result, JsonOptions),
        };
    }

    [McpServerTool(Name = "terminal.which")]
    [Description(
        "TYPE 2 — Check whether a command exists on PATH. " +
        "Any phase agent may call (no HIL required). " +
        "Planner MUST call this before writing RunCommand steps that depend on external tools.")]
    public async Task<TerminalResult> Which(
        [Description("Command to locate, e.g. 'dotnet', 'git', 'node', 'python', 'playwright'.")] string command,
        CancellationToken cancellationToken = default)
    {
        var whichCmd = OperatingSystem.IsWindows() ? $"where {command}" : $"which {command}";
        return await RunCommand("terminal-1", whichCmd, "", null, cancellationToken);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private string? ResolveWorkingDir(string relative)
    {
        var candidate = string.IsNullOrWhiteSpace(relative)
            ? config.WorkingRoot
            : Path.GetFullPath(Path.Combine(config.WorkingRoot, relative));

        return candidate.StartsWith(config.WorkingRoot, StringComparison.OrdinalIgnoreCase)
            ? candidate
            : null;
    }

    private ProcessStartInfo BuildProcessStartInfo(string command, string workingDir, string[]? env)
    {
        var psi = new ProcessStartInfo
        {
            FileName               = config.Shell,
            Arguments              = $"{config.ShellCommandFlag} {command}",
            WorkingDirectory       = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };

        if (env is { Length: > 0 })
        {
            foreach (var kv in env)
            {
                var eq = kv.IndexOf('=');
                if (eq > 0) psi.Environment[kv[..eq]] = kv[(eq + 1)..];
            }
        }

        return psi;
    }

    private static TerminalResult Err(string message) =>
        new() { Success = false, Error = message };

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
}

// ── Result contract ───────────────────────────────────────────────────────────

/// <summary>
/// I Am the TerminalResult. I am the structured output contract for all terminal
/// tool calls. I ensure the LLM always receives typed, predictable data.
/// </summary>
public sealed class TerminalResult
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Success { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Slot { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Command { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ExitCode { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Stdout { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Stderr { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? WorkingDir { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTimeOffset ExecutedAt { get; init; }
}
