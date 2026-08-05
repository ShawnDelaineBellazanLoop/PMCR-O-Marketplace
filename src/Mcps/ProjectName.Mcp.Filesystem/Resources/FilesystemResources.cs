// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.FILESYSTEM
// File       : Resources/FilesystemResources.cs
// Identity   : MCP Pillar 2 — Resources (agent-readable contextual manifests)
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, EC-005, SAFETY-FS-001, ANTHROPIC-AGENT-001
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design:
//   Resources are the "memory" layer — they give the agent context BEFORE action.
//   filesystem://roots    → agent reads this to know where it can operate
//   filesystem://config   → agent reads this to know limits (size, depth, entries)
//   filesystem://skill    → agent reads this once per session for the full contract
//   filesystem://stat/{p} → agent reads this to get augmented metadata on any path
//
//   Every resource includes a next_actions hint so the agent knows what to do
//   after reading it (ANTHROPIC-AGENT-001 — eliminate "what now?" loops).
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using ProjectName.Mcp.Filesystem.Configuration;

using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ProjectName.Mcp.Filesystem.Resources;

/// <summary>
/// I Am the Filesystem MCP Resource Provider. I am the "Memory" layer of the
/// ProjectName.Mcp.Filesystem server — Pillar 2 of the three MCP primitives.
/// I expose read-only contextual data so agents understand the filesystem
/// execution envelope before issuing any write or read command.
/// All my resources are TYPE 2 — no HIL required, any agent may read me (EC-002).
/// </summary>
[McpServerResourceType]
public sealed class FilesystemResources(FilesystemConfig config, ILogger<FilesystemResources> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly Action<ILogger, string, Exception?> _logRead =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(20, "ResRead"), "[FS-RES] {Uri} fetched");
    private static readonly Action<ILogger, string, Exception?> _logSkillFallback =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(21, "SkillFallback"),
            "[FS-RES] SKILL.md not found at {Path} — returning inline fallback");

    // ════════════════════════════════════════════════════════════════════════
    // Direct Resources — fixed URI, always in resources/list
    // ════════════════════════════════════════════════════════════════════════

    [McpServerResource(
        UriTemplate = "filesystem://roots",
        Name        = "Filesystem Allowed Roots",
        MimeType    = "application/json")]
    [Description(
        "TYPE 2 — Returns the AllowedRoots sandbox contract. " +
        "Agent MUST read this before planning any file operation. " +
        "All paths passed to filesystem tools must be under one of these roots.")]
    public TextResourceContents GetRoots()
    {
        _logRead(logger, "filesystem://roots", null);

        var payload = new
        {
            thoughtlock  = "2026-05-30",
            allowed_roots = config.AllowedRoots.Select(r => new
            {
                path    = r,
                exists  = Directory.Exists(r),
                summary = $"Root '{r}' — {(Directory.Exists(r) ? "accessible" : "NOT FOUND on disk")}",
            }).ToArray(),
            denied_patterns = config.DeniedPatterns,
            law_anchor      = "SAFETY-FS-001",
            agent_note      = "All paths passed to filesystem tools must start with one of the allowed_roots entries. " +
                              "Denied patterns are blocked even within allowed roots.",
            next_actions = new[]
            {
                "Read filesystem://config for size and entry limits",
                "Use filesystem.list_directory to enumerate a root",
                "Use filesystem.file_exists before reading uncertain paths",
            },
        };

        return Json("filesystem://roots", payload);
    }

    [McpServerResource(
        UriTemplate = "filesystem://config",
        Name        = "Filesystem Server Config",
        MimeType    = "application/json")]
    [Description(
        "TYPE 2 — Returns the full filesystem server configuration: " +
        "AllowedRoots, MaxFileSizeBytes, MaxListEntries, MaxRecursionDepth, DeniedPatterns. " +
        "Agent reads this to understand capability limits before planning bulk operations.")]
    public TextResourceContents GetConfig()
    {
        _logRead(logger, "filesystem://config", null);

        var payload = new
        {
            thoughtlock          = "2026-05-30",
            allowed_roots        = config.AllowedRoots,
            max_file_size_bytes  = config.MaxFileSizeBytes,
            max_file_size_human  = $"{config.MaxFileSizeBytes / 1024 / 1024} MB",
            max_list_entries     = config.MaxListEntries,
            max_recursion_depth  = config.MaxRecursionDepth,
            denied_patterns      = config.DeniedPatterns,
            type_boundary = new
            {
                type1_tools = new[] { "filesystem.read_file", "filesystem.write_file", "filesystem.delete_file", "filesystem.move_file" },
                type2_tools = new[] { "filesystem.list_directory", "filesystem.file_exists", "filesystem.get_info" },
                note        = "TYPE 1 tools require Orchestrator + HIL approval (EC-002, MAAI-001). Resources always TYPE 2.",
            },
            law_anchors  = new[] { "SAFETY-FS-001", "EC-002", "MAAI-001", "FRAC-MCP-400-001" },
            next_actions = new[]
            {
                "Use filesystem.list_directory to map the directory structure",
                "Use filesystem.file_exists before attempting to read unknown paths",
                "Read filesystem://skill for the complete capability contract",
            },
        };

        return Json("filesystem://config", payload);
    }

    [McpServerResource(
        UriTemplate = "filesystem://skill",
        Name        = "Filesystem MCP SKILL.md",
        MimeType    = "text/markdown")]
    [Description(
        "TYPE 2 — Returns the SKILL.md capability manifest for this Filesystem MCP server. " +
        "Agent reads this once per session before issuing the first filesystem tool call.")]
    public TextResourceContents GetSkill()
    {
        _logRead(logger, "filesystem://skill", null);

        var assemblyDir = Path.GetDirectoryName(typeof(FilesystemResources).Assembly.Location)
                          ?? AppContext.BaseDirectory;
        var skillPath   = Path.Combine(assemblyDir, "skills", "filesystem-mcp", "SKILL.md");

        if (File.Exists(skillPath))
        {
            return new TextResourceContents
            {
                Uri      = "filesystem://skill",
                MimeType = "text/markdown",
                Text     = File.ReadAllText(skillPath),
            };
        }

        _logSkillFallback(logger, skillPath, null);
        return new TextResourceContents
        {
            Uri      = "filesystem://skill",
            MimeType = "text/markdown",
            Text     = GetInlineSkillFallback(),
        };
    }

    // ════════════════════════════════════════════════════════════════════════
    // Templated Resources — URI template, appear in resources/templates/list
    // ════════════════════════════════════════════════════════════════════════

    [McpServerResource(
        UriTemplate = "filesystem://stat/{path}",
        Name        = "Filesystem Path Stat",
        MimeType    = "application/json")]
    [Description(
        "TYPE 2 — Returns augmented stat for any sandbox path: exists, kind, size, line_count, " +
        "detected_language, last_modified, and next_actions. " +
        "Agent reads this for path metadata without consuming a full read_file call. " +
        "Path must be URL-encoded if it contains backslashes.")]
    public TextResourceContents GetStat(
        [Description("The path to stat — URL-encoded absolute path under an AllowedRoot.")] string path)
    {
        _logRead(logger, $"filesystem://stat/{path}", null);

        // URL-decode and normalize separators
        var decoded    = Uri.UnescapeDataString(path).Replace('/', '\\');
        var normalized = System.IO.Path.GetFullPath(decoded);

        if (!config.IsPathAllowed(normalized))
            throw new McpException(config.SandboxViolationMessage(normalized));

        var isFile = File.Exists(normalized);
        var isDir  = Directory.Exists(normalized);

        if (!isFile && !isDir)
        {
            var payload = new
            {
                thoughtlock  = "2026-05-30",
                path         = normalized,
                exists       = false,
                kind         = "none",
                next_actions = new[] { "Use filesystem.list_directory on the parent", "Use filesystem.write_file to create the file" },
            };
            return Json($"filesystem://stat/{path}", payload);
        }

        if (isFile)
        {
            var fi       = new FileInfo(normalized);
            var lang     = DetectLanguage(normalized);
            var tooLarge = fi.Length > config.MaxFileSizeBytes;

            int? lineCount = null;
            if (!tooLarge && IsTextExtension(normalized))
            {
                try { lineCount = File.ReadAllLines(normalized).Length; } catch { /* best-effort */ }
            }

            var filePayload = new
            {
                thoughtlock       = "2026-05-30",
                path              = normalized,
                exists            = true,
                kind              = "file",
                name              = fi.Name,
                extension         = fi.Extension,
                detected_language = lang,
                size_bytes        = fi.Length,
                size_human        = FormatBytes(fi.Length),
                line_count        = lineCount,
                is_too_large      = tooLarge,
                max_size_bytes    = config.MaxFileSizeBytes,
                last_modified_utc = fi.LastWriteTimeUtc,
                created_utc       = fi.CreationTimeUtc,
                next_actions      = tooLarge
                    ? new[] { "Use filesystem.read_file with fromLine/toLine to chunk" }
                    : new[] { "Use filesystem.read_file to read full content" },
            };
            return Json($"filesystem://stat/{path}", filePayload);
        }
        else
        {
            var di       = new DirectoryInfo(normalized);
            var topFiles = Directory.GetFiles(normalized).Length;
            var topDirs  = Directory.GetDirectories(normalized).Length;

            var dirPayload = new
            {
                thoughtlock       = "2026-05-30",
                path              = normalized,
                exists            = true,
                kind              = "directory",
                name              = di.Name,
                top_level_files   = topFiles,
                top_level_dirs    = topDirs,
                last_modified_utc = di.LastWriteTimeUtc,
                created_utc       = di.CreationTimeUtc,
                next_actions      = new[] { "Use filesystem.list_directory to enumerate contents" },
            };
            return Json($"filesystem://stat/{path}", dirPayload);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TextResourceContents Json(string uri, object payload) =>
        new()
        {
            Uri      = uri,
            MimeType = "application/json",
            Text     = JsonSerializer.Serialize(payload, JsonOptions),
        };

    private static string FormatBytes(long bytes) => bytes switch
    {
        < 1024                => $"{bytes} B",
        < 1024 * 1024         => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024  => $"{bytes / 1024.0 / 1024:F1} MB",
        _                     => $"{bytes / 1024.0 / 1024 / 1024:F2} GB",
    };

    private static string DetectLanguage(string path) =>
        System.IO.Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".cs"      => "C#",
            ".csproj"  => "MSBuild/XML",
            ".json"    => "JSON",
            ".md"      => "Markdown",
            ".yaml" or ".yml" => "YAML",
            ".xml"     => "XML",
            ".ts"      => "TypeScript",
            ".js"      => "JavaScript",
            ".py"      => "Python",
            ".sh"      => "Shell",
            ".ps1"     => "PowerShell",
            ".sql"     => "SQL",
            ".html"    => "HTML",
            ".css"     => "CSS",
            ".props" or ".targets" => "MSBuild",
            _          => "plaintext",
        };

    private static bool IsTextExtension(string path) =>
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".csproj", ".json", ".md", ".yaml", ".yml", ".xml", ".ts", ".tsx",
            ".js", ".py", ".sh", ".ps1", ".sql", ".html", ".css", ".toml", ".props",
            ".targets", ".txt", ".log", ".gitignore", ".dockerignore",
        }.Contains(System.IO.Path.GetExtension(path));

    private static string GetInlineSkillFallback() =>
        """
        ---
        name: filesystem-mcp
        tier: SHARED — Pillar 3 Infrastructure
        type_boundary:
          type1: [filesystem.read_file, filesystem.write_file, filesystem.delete_file, filesystem.move_file]
          type2: [filesystem.list_directory, filesystem.file_exists, filesystem.get_info]
        resources:
          - filesystem://roots       # AllowedRoots sandbox contract — read first
          - filesystem://config      # Limits: MaxFileSizeBytes, MaxListEntries, MaxRecursionDepth
          - filesystem://skill       # This document
          - filesystem://stat/{path} # Augmented stat for any path
        prompts:
          - filesystem-read-plan
          - filesystem-write-scaffold
          - filesystem-debug-access
        anthropic_agent_design:
          extract_and_summarize: Every FileResult includes .summary, .structured, .lines, .next_actions
          poka_yoke: Sandbox enforced at FilesystemConfig.IsPathAllowed() — single choke-point
          pre_flight: Read filesystem://roots then filesystem://config before any tool call
        law_anchors: [SAFETY-FS-001, EC-002, MAAI-001, ANTHROPIC-ACI-001, ANTHROPIC-AGENT-001]
        thoughtlock: "2026-05-30"
        ---
        Full SKILL.md not found. Deploy skills/filesystem-mcp/SKILL.md adjacent to the assembly.
        """;
}
