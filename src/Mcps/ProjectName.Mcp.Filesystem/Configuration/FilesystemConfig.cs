// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.FILESYSTEM
// File       : Configuration/FilesystemConfig.cs
// Identity   : Filesystem sandbox boundary and capability configuration
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : SAFETY-FS-001 (AllowedRoots sandbox), EC-002 (TYPE 1/2 boundary),
//              ANTHROPIC-ACI-001 (Poka-yoke — bad paths fail at config, not at I/O)
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design applied here:
//   The sandbox boundary is structural, not scattered across tools.
//   IsPathAllowed() is the single choke-point — every tool calls it once.
//   Agents read filesystem://config to know AllowedRoots before planning.
//   DeniedPatterns prevent secret exfiltration even on valid root paths.
// ═══════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace ProjectName.Mcp.Filesystem.Configuration;

/// <summary>
/// I Am the Filesystem MCP configuration. I define the sandbox boundary for all
/// file I/O operations. I enforce the AllowedRoots contract (SAFETY-FS-001) and
/// expose the capability envelope that agents read via filesystem://config before
/// planning any file operations. I am injected as a singleton at startup.
/// </summary>
public sealed class FilesystemConfig
{
    /// <summary>
    /// Absolute paths the Filesystem MCP may read from or write to.
    /// Any path not under one of these roots is rejected before I/O (SAFETY-FS-001).
    /// Injected by Aspire: Filesystem__AllowedRoots__0, __1, … in AppHost.cs.
    /// </summary>
    [Required, MinLength(1)]
    public string[] AllowedRoots { get; init; } = [@"A:\PMCR-O"];

    /// <summary>
    /// Maximum file size for a single read or write operation.
    /// Files exceeding this limit return a structured error with the actual byte
    /// count so the agent can choose to stream or split (Poka-yoke, never silent OOM).
    /// Default: 10 MB.
    /// </summary>
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;

    /// <summary>
    /// Maximum directory entries returned by a single list_directory call.
    /// Prevents token-bloat when listing node_modules or bin folders.
    /// Response includes total_count so the agent knows data was truncated.
    /// </summary>
    public int MaxListEntries { get; init; } = 500;

    /// <summary>
    /// Maximum recursion depth for tree-walk operations.
    /// Prevents infinite traversal of deep or circular symlink structures.
    /// </summary>
    public int MaxRecursionDepth { get; init; } = 10;

    /// <summary>
    /// Glob patterns that are always denied regardless of AllowedRoots.
    /// Structural Poka-yoke: credentials and secrets are never readable even when
    /// the agent constructs a technically valid allowed path.
    /// </summary>
    public string[] DeniedPatterns { get; init; } =
    [
        "**/.git/config",
        "**/.env",
        "**/*.pfx",
        "**/*.p12",
        "**/*.key",
        "**/secrets.json",
        "**/usersecrets/**",
        "**/.npmrc",
        "**/.pypirc",
        "**/appsettings.Production.json",
    ];

    // ── Sandbox gate — single choke-point (SAFETY-FS-001) ───────────────────

    /// <summary>
    /// Returns true if <paramref name="absolutePath"/> is under an AllowedRoot
    /// AND does not match any DeniedPattern.
    /// Every tool calls this exactly once before any I/O. Never bypass.
    /// </summary>
    public bool IsPathAllowed(string absolutePath)
    {
        var normalized = Path.GetFullPath(absolutePath);

        var underRoot = AllowedRoots.Any(root =>
            normalized.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase));

        if (!underRoot) return false;

        return !DeniedPatterns.Any(pattern => GlobMatch(normalized.Replace('\\', '/'), pattern.Replace('\\', '/')));
    }

    /// <summary>
    /// Returns the matched AllowedRoot for display in agent error messages,
    /// so the agent sees which roots are valid without calling filesystem://config separately.
    /// </summary>
    public string? GetMatchedRoot(string absolutePath)
    {
        var normalized = Path.GetFullPath(absolutePath);
        return AllowedRoots.FirstOrDefault(root =>
            normalized.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns a sandbox-violation error string formatted for agent consumption.
    /// Includes AllowedRoots so the agent can self-correct without a separate tool call.
    /// </summary>
    public string SandboxViolationMessage(string attemptedPath) =>
        $"SANDBOX-VIOLATION: '{attemptedPath}' is outside allowed roots or matches a denied pattern. " +
        $"AllowedRoots: [{string.Join(", ", AllowedRoots)}]. " +
        $"Read filesystem://config for the full boundary contract (SAFETY-FS-001).";

    // ── Glob helpers ─────────────────────────────────────────────────────────

    private static bool GlobMatch(string path, string pattern)
    {
        // ** matches any depth
        if (pattern.Contains("**"))
        {
            var parts = pattern.Split("**", StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ||
                   (parts.Length >= 1 && path.Contains(parts[^1].Trim('/'), StringComparison.OrdinalIgnoreCase));
        }
        // * matches single segment
        if (pattern.Contains('*'))
        {
            var segs = pattern.Split('*');
            return segs.All(s => string.IsNullOrEmpty(s) || path.Contains(s, StringComparison.OrdinalIgnoreCase));
        }
        return path.EndsWith(pattern.TrimStart('/'), StringComparison.OrdinalIgnoreCase);
    }
}
