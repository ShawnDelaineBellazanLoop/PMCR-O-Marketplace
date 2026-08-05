// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.TERMINAL
// File       : Configuration/TerminalConfig.cs
// Identity   : Terminal sandbox and execution configuration manifest
// Pillar     : 3 — Infrastructure
// Law Anchor : SAFETY-003 (sandbox root enforcement), EC-002 (TYPE boundary),
//              ANTHROPIC-ACI-001 (Poka-yoke tool design)
// ThoughtLock: 2026-05-30
//
// Anthropic ACI Principle applied here:
//   "Design tools so the model cannot fail by construction."
//   WorkingRoot is the structural Poka-yoke — the sandbox boundary is enforced
//   at the config layer, before any command reaches the shell. The LLM never
//   sees raw filesystem paths; it sees slot names and relative paths only.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ProjectName.Mcp.Terminal.Configuration;

/// <summary>
/// I Am the Terminal MCP configuration. I define the execution sandbox for all
/// shell commands. I enforce the WorkingRoot boundary (SAFETY-003) and the
/// per-slot timeout model. I am injected as a singleton at startup via Aspire
/// environment variables from AppHost.cs.
/// </summary>
public sealed class TerminalConfig
{
    /// <summary>
    /// Absolute path that serves as the sandbox root for all command execution.
    /// No command may resolve a working directory above this path (SAFETY-003 / Poka-yoke).
    /// Injected by Aspire: Parameters__working-root → AppHost WithEnvironment().
    /// </summary>
    public required string WorkingRoot { get; init; }

    /// <summary>
    /// Default maximum wall-clock seconds a single command may run before cancellation.
    /// Per-slot overrides in <see cref="SlotTimeouts"/> take precedence.
    /// </summary>
    public int CommandTimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Maximum bytes captured from stdout per command invocation.
    /// Output beyond this limit is truncated with an explicit marker — the LLM
    /// always knows when output was cut (Poka-yoke: no silent data loss).
    /// </summary>
    public int MaxOutputBytes { get; init; } = 65536;

    /// <summary>
    /// Shell executable resolved at runtime.
    /// cmd.exe on Windows, /bin/bash on Linux — matches Docker base image (Linux).
    /// </summary>
    public string Shell { get; init; } =
        OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash";

    /// <summary>
    /// Shell flag for inline command execution.
    /// /C for cmd.exe, -c for bash.
    /// </summary>
    public string ShellCommandFlag { get; init; } =
        OperatingSystem.IsWindows() ? "/C" : "-c";

    /// <summary>
    /// Per-slot timeout overrides. terminal-4 (scraper/long-running) defaults to
    /// 900s (15 min) to accommodate Playwright and long dotnet operations.
    /// All other slots use <see cref="CommandTimeoutSeconds"/>.
    /// </summary>
    public Dictionary<string, int> SlotTimeouts { get; init; } = new()
    {
        ["terminal-4"] = 900,
    };

    /// <summary>Returns the effective timeout in seconds for the given slot name.</summary>
    public int GetTimeoutSeconds(string slot) =>
        SlotTimeouts.TryGetValue(slot, out var t) ? t : CommandTimeoutSeconds;
}
