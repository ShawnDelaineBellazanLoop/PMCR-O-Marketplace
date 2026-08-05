// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Configuration/OrchestratorConfig.cs
// Identity   : Typed configuration for the PMCRO orchestrator
// Law Anchor : EC-009 (MaxLoops MUST be set before any cycle opens)
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace ProjectName.AgentService.Configuration;

/// <summary>
/// EC-009: All orchestrator limits must be expressed as typed configuration,
/// not magic numbers or un-read environment variables.
/// </summary>
public sealed class OrchestratorConfig
{
    public const string SectionName = "Orchestrator";

    /// <summary>Maximum PMCRO loops before escalation. Must be ≥ 1. Default: 3.</summary>
    [Range(1, 20)]
    public int MaxLoops { get; init; } = 3;

    /// <summary>Timeout per individual phase invocation in ms. Default: 60 s.</summary>
    [Range(1000, 600_000)]
    public int PhaseTimeoutMs { get; init; } = 60_000;

    /// <summary>Total cycle timeout across all loops in ms. Default: 5 min.</summary>
    [Range(5000, 3_600_000)]
    public int DefaultTimeoutMs { get; init; } = 300_000;

    /// <summary>
    /// Absolute filesystem root for trail and governance file writes.
    /// Injected by Aspire via Orchestrator__FileSystemRoot.
    /// </summary>
    public string FileSystemRoot { get; init; } = Directory.GetCurrentDirectory();

    /// <summary>Path relative to FileSystemRoot where trail JSONL files are written.</summary>
    public string TrailDirectory { get; init; } = ".pmcro/trails";

    /// <summary>Returns the absolute path to the trail root directory.</summary>
    public string AbsoluteTrailRoot =>
        Path.GetFullPath(Path.Combine(FileSystemRoot, TrailDirectory));
}
