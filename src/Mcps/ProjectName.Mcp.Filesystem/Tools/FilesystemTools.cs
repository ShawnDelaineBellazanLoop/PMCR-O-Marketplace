// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.FILESYSTEM
// File       : Tools/FilesystemTools.cs
// Identity   : Filesystem Actuator — atomic file I/O with agent-augmented returns
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, SAFETY-FS-001, ANTHROPIC-ACI-001, ANTHROPIC-AGENT-001
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design — Extract + Summarize pattern:
//   Every tool return includes:
//     success      — boolean gate the agent checks first
//     summary      — one-sentence natural language the agent can use in reasoning
//     structured   — typed data the agent can address by field without re-parsing
//     raw / lines  — verbatim content when the agent needs it
//     next_actions — agent-readable hints for what to do next (ANTHROPIC-AGENT-001)
//   This eliminates "read output → re-summarize" loops in the agent.
//   The agent reads .summary to reason; it reads .structured to act.
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using ProjectName.Mcp.Filesystem.Configuration;

using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace ProjectName.Mcp.Filesystem.Tools;

/// <summary>
/// I Am the Filesystem MCP Tool Provider. I am the "File I/O Hands" of the PMCR-O
/// cognitive stack. I expose atomic, sandbox-enforced file operations with
/// augmented returns — every result includes a summary and structured data the
/// agent can reason over without additional parsing loops.
/// </summary>
[McpServerToolType]
public sealed class FilesystemTools(FilesystemConfig config, ILogger<FilesystemTools> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ── LoggerMessage delegates (CA1848) ─────────────────────────────────────
    private static readonly Action<ILogger, string, Exception?> _logRead =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "Read"), "[FS] read_file: {Path}");
    private static readonly Action<ILogger, string, Exception?> _logWrite =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, "Write"), "[FS] write_file: {Path}");
    private static readonly Action<ILogger, string, Exception?> _logDelete =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(3, "Delete"), "[FS] delete_file: {Path}");
    private static readonly Action<ILogger, string, string, Exception?> _logMove =
        LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(4, "Move"), "[FS] move_file: {Src} → {Dst}");
    private static readonly Action<ILogger, string, Exception?> _logList =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(5, "List"), "[FS] list_directory: {Path}");
    private static readonly Action<ILogger, string, Exception?> _logFault =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(9, "Fault"), "[FS] fault: {Msg}");

    // ════════════════════════════════════════════════════════════════════════
    // TYPE 1 — World-changing (write / delete / move)
    // Orchestrator + HIL approval required (EC-002, MAAI-001).
    // ════════════════════════════════════════════════════════════════════════

    [McpServerTool(Name = "filesystem.read_file")]
    [Description(
        "TYPE 1 — Read a file within the sandbox. Returns structured FileResult with " +
        "summary, line_count, char_count, detected_language, and lines array. " +
        "Use filesystem.file_exists (TYPE 2) first if existence is uncertain. " +
        "Files over MaxFileSizeBytes return a structured error with the actual size. " +
        "Sandbox enforced: path must be under an AllowedRoot (SAFETY-FS-001).")]
    public async Task<FileResult> ReadFile(
        [Description("Absolute path to the file. Must be under an AllowedRoot.")] string path,
        [Description("Optional: return only lines from this line number (1-based, inclusive).")] int? fromLine = null,
        [Description("Optional: return only lines up to this line number (1-based, inclusive).")] int? toLine = null,
        CancellationToken cancellationToken = default)
    {
        if (!config.IsPathAllowed(path))
            return Err(config.SandboxViolationMessage(path));

        if (!File.Exists(path))
            return Err($"File not found: '{path}'. Use filesystem.list_directory to verify the path exists.");

        try
        {
            var info = new FileInfo(path);
            if (info.Length > config.MaxFileSizeBytes)
                return Err(
                    $"File too large: {info.Length:N0} bytes exceeds MaxFileSizeBytes ({config.MaxFileSizeBytes:N0}). " +
                    $"Use fromLine/toLine to read in chunks, or use filesystem.get_info to inspect metadata first.");

            _logRead(logger, path, null);

            var allLines = await File.ReadAllLinesAsync(path, cancellationToken);
            var selectedLines = (fromLine, toLine) switch
            {
                (null, null) => allLines,
                (int f, null) => allLines.Skip(f - 1).ToArray(),
                (null, int t) => allLines.Take(t).ToArray(),
                (int f, int t) => allLines.Skip(f - 1).Take(t - f + 1).ToArray(),
            };

            var lang = DetectLanguage(path);
            var charCount = selectedLines.Sum(l => l.Length);
            var isPartial = fromLine.HasValue || toLine.HasValue;

            return new FileResult
            {
                Success    = true,
                Path       = path,
                Summary    = $"Read {selectedLines.Length:N0} lines ({charCount:N0} chars) from '{Path.GetFileName(path)}'" +
                             (isPartial ? $" [lines {fromLine ?? 1}–{toLine ?? allLines.Length}]" : "") +
                             $" — {lang}.",
                Structured = new
                {
                    file_name          = Path.GetFileName(path),
                    extension          = Path.GetExtension(path),
                    detected_language  = lang,
                    total_line_count   = allLines.Length,
                    returned_line_count = selectedLines.Length,
                    char_count         = charCount,
                    is_partial_read    = isPartial,
                    size_bytes         = info.Length,
                    last_modified_utc  = info.LastWriteTimeUtc,
                },
                Lines      = selectedLines,
                NextActions = isPartial
                    ? [$"Read next chunk: fromLine={toLine + 1}", "Use filesystem.write_file to modify content"]
                    : ["Use filesystem.write_file to modify content", "Pass Lines to agent reasoning context"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Read fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "filesystem.write_file")]
    [Description(
        "TYPE 1 — Write (create or overwrite) a file within the sandbox. " +
        "Orchestrator + HIL approval required (EC-002, MAAI-001). " +
        "Creates parent directories automatically if they do not exist. " +
        "Returns structured result with bytes_written and existence_before flag so " +
        "the agent knows whether it created or overwrote. " +
        "Sandbox enforced: path must be under an AllowedRoot (SAFETY-FS-001).")]
    public async Task<FileResult> WriteFile(
        [Description("Absolute path to write. Must be under an AllowedRoot.")] string path,
        [Description("Full content to write. Overwrites any existing file.")] string content,
        [Description("Encoding: utf-8 (default) | utf-8-bom | ascii")] string encoding = "utf-8",
        CancellationToken cancellationToken = default)
    {
        if (!config.IsPathAllowed(path))
            return Err(config.SandboxViolationMessage(path));

        try
        {
            var existedBefore = File.Exists(path);
            var enc = encoding.ToLowerInvariant() switch
            {
                "utf-8-bom" => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
                "ascii"     => Encoding.ASCII,
                _           => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            };

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            _logWrite(logger, path, null);
            await File.WriteAllTextAsync(path, content, enc, cancellationToken);

            var bytes = (long)enc.GetByteCount(content);
            var lines = content.Split('\n').Length;

            return new FileResult
            {
                Success    = true,
                Path       = path,
                Summary    = $"{(existedBefore ? "Overwrote" : "Created")} '{Path.GetFileName(path)}' — {bytes:N0} bytes, {lines:N0} lines.",
                Structured = new
                {
                    file_name      = Path.GetFileName(path),
                    existed_before = existedBefore,
                    bytes_written  = bytes,
                    lines_written  = lines,
                    encoding       = encoding,
                    written_at_utc = DateTimeOffset.UtcNow,
                },
                NextActions =
                [
                    "Use filesystem.read_file to verify written content",
                    "Use terminal.run_command to build/test if this was source code",
                ],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Write fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "filesystem.delete_file")]
    [Description(
        "TYPE 1 — Permanently delete a file or empty directory within the sandbox. " +
        "Orchestrator + HIL approval required (EC-002, MAAI-001). " +
        "Non-recoverable. Returns structured result confirming what was deleted. " +
        "Sandbox enforced: path must be under an AllowedRoot (SAFETY-FS-001).")]
    public FileResult DeleteFile(
        [Description("Absolute path to delete. Must be under an AllowedRoot.")] string path,
        [Description("If true, delete a directory and all its contents recursively. Default false (fail on non-empty dir).")] bool recursive = false)
    {
        if (!config.IsPathAllowed(path))
            return Err(config.SandboxViolationMessage(path));

        try
        {
            bool isDir = Directory.Exists(path);
            bool isFile = File.Exists(path);

            if (!isFile && !isDir)
                return Err($"Path not found: '{path}'. Nothing was deleted.");

            long sizeBytes = 0;
            int fileCount  = 0;

            if (isFile)
            {
                sizeBytes = new FileInfo(path).Length;
                fileCount = 1;
                _logDelete(logger, path, null);
                File.Delete(path);
            }
            else
            {
                if (!recursive)
                    return Err($"'{path}' is a directory. Set recursive=true to delete it and all contents.");
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                fileCount = files.Length;
                sizeBytes = files.Sum(f => new FileInfo(f).Length);
                _logDelete(logger, path, null);
                Directory.Delete(path, recursive: true);
            }

            return new FileResult
            {
                Success    = true,
                Path       = path,
                Summary    = $"Deleted {(isDir ? "directory" : "file")} '{Path.GetFileName(path)}' — {fileCount} file(s), {sizeBytes:N0} bytes freed.",
                Structured = new
                {
                    deleted_path   = path,
                    was_directory  = isDir,
                    files_deleted  = fileCount,
                    bytes_freed    = sizeBytes,
                    deleted_at_utc = DateTimeOffset.UtcNow,
                },
                NextActions = ["Use filesystem.list_directory on the parent to confirm deletion"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Delete fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "filesystem.move_file")]
    [Description(
        "TYPE 1 — Move or rename a file or directory within the sandbox. " +
        "Orchestrator + HIL approval required (EC-002, MAAI-001). " +
        "Both source and destination must be under AllowedRoots. " +
        "Creates destination parent directories automatically. " +
        "Sandbox enforced: both paths must be under AllowedRoots (SAFETY-FS-001).")]
    public FileResult MoveFile(
        [Description("Absolute source path. Must be under an AllowedRoot.")] string sourcePath,
        [Description("Absolute destination path. Must be under an AllowedRoot.")] string destinationPath,
        [Description("If true and destination exists, overwrite it. Default false.")] bool overwrite = false)
    {
        if (!config.IsPathAllowed(sourcePath))
            return Err(config.SandboxViolationMessage(sourcePath));
        if (!config.IsPathAllowed(destinationPath))
            return Err(config.SandboxViolationMessage(destinationPath));

        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            return Err($"Source not found: '{sourcePath}'.");

        try
        {
            var destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);

            _logMove(logger, sourcePath, destinationPath, null);

            if (File.Exists(sourcePath))
                File.Move(sourcePath, destinationPath, overwrite);
            else
                Directory.Move(sourcePath, destinationPath);

            return new FileResult
            {
                Success    = true,
                Path       = destinationPath,
                Summary    = $"Moved '{Path.GetFileName(sourcePath)}' → '{Path.GetFileName(destinationPath)}'.",
                Structured = new
                {
                    source      = sourcePath,
                    destination = destinationPath,
                    moved_at_utc = DateTimeOffset.UtcNow,
                },
                NextActions = ["Use filesystem.file_exists to confirm new location", "Use filesystem.read_file to verify content"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"Move fault: {ex.Message}");
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // TYPE 2 — Read-only inspection (no HIL required, any agent may call)
    // ════════════════════════════════════════════════════════════════════════

    [McpServerTool(Name = "filesystem.list_directory")]
    [Description(
        "TYPE 2 — List files and directories at path. No HIL required. " +
        "Returns structured result: files[], directories[], counts, and a summary " +
        "the agent can use directly in planning. " +
        "Results capped at MaxListEntries — total_count shows true entry count. " +
        "Sandbox enforced: path must be under an AllowedRoot (SAFETY-FS-001).")]
    public FileResult ListDirectory(
        [Description("Absolute directory path. Must be under an AllowedRoot.")] string path,
        [Description("Include files matching this glob pattern. Empty = all. Example: '*.cs'")] string pattern = "",
        [Description("If true, list recursively up to MaxRecursionDepth.")] bool recursive = false)
    {
        if (!config.IsPathAllowed(path))
            return Err(config.SandboxViolationMessage(path));

        if (!Directory.Exists(path))
            return Err($"Directory not found: '{path}'. Use filesystem.file_exists to check if it exists.");

        try
        {
            _logList(logger, path, null);

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var allFiles     = string.IsNullOrEmpty(pattern)
                ? Directory.GetFiles(path, "*", searchOption)
                : Directory.GetFiles(path, pattern, searchOption);
            var allDirs      = Directory.GetDirectories(path, "*", searchOption);

            var files   = allFiles.Take(config.MaxListEntries).Select(f =>
            {
                var fi = new FileInfo(f);
                return new { name = fi.Name, path = f, size_bytes = fi.Length, extension = fi.Extension, last_modified_utc = fi.LastWriteTimeUtc };
            }).ToArray();

            var dirs = allDirs.Take(Math.Max(0, config.MaxListEntries - files.Length)).Select(d =>
                new { name = Path.GetFileName(d), path = d }).ToArray();

            var truncated = (allFiles.Length + allDirs.Length) > config.MaxListEntries;

            return new FileResult
            {
                Success    = true,
                Path       = path,
                Summary    = $"'{Path.GetFileName(path)}' contains {allFiles.Length} file(s) and {allDirs.Length} dir(s)" +
                             (truncated ? $" — TRUNCATED to {config.MaxListEntries} entries" : "") +
                             (string.IsNullOrEmpty(pattern) ? "" : $" (filter: {pattern})") + ".",
                Structured = new
                {
                    directory       = path,
                    file_count      = allFiles.Length,
                    directory_count = allDirs.Length,
                    total_count     = allFiles.Length + allDirs.Length,
                    is_truncated    = truncated,
                    max_entries     = config.MaxListEntries,
                    is_recursive    = recursive,
                    pattern_filter  = pattern,
                    files,
                    directories     = dirs,
                },
                NextActions = truncated
                    ? ["Use pattern filter to narrow results", "Use recursive=false to reduce scope"]
                    : ["Use filesystem.read_file on any file of interest"],
            };
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"List fault: {ex.Message}");
        }
    }

    [McpServerTool(Name = "filesystem.file_exists")]
    [Description(
        "TYPE 2 — Check whether a file or directory exists within the sandbox. No HIL required. " +
        "Returns exists (bool), kind (file|directory|none), size_bytes, and last_modified_utc. " +
        "Call this before read_file or write_file when existence is uncertain (Poka-yoke).")]
    public FileResult FileExists(
        [Description("Absolute path to check. Must be under an AllowedRoot.")] string path)
    {
        if (!config.IsPathAllowed(path))
            return Err(config.SandboxViolationMessage(path));

        var isFile = File.Exists(path);
        var isDir  = Directory.Exists(path);
        var kind   = isFile ? "file" : isDir ? "directory" : "none";

        long? sizeBytes = isFile ? new FileInfo(path).Length : null;
        DateTimeOffset? lastMod = isFile ? new FileInfo(path).LastWriteTimeUtc
                                 : isDir  ? new DirectoryInfo(path).LastWriteTimeUtc : null;

        return new FileResult
        {
            Success    = true,
            Path       = path,
            Summary    = $"'{Path.GetFileName(path)}' {(kind == "none" ? "does not exist" : $"exists as a {kind}")}." +
                         (sizeBytes.HasValue ? $" Size: {sizeBytes:N0} bytes." : ""),
            Structured = new
            {
                path,
                exists           = kind != "none",
                kind,
                size_bytes       = sizeBytes,
                last_modified_utc = lastMod,
            },
            NextActions = kind switch
            {
                "file"      => ["Use filesystem.read_file to read content"],
                "directory" => ["Use filesystem.list_directory to enumerate contents"],
                _           => ["Use filesystem.write_file to create the file", "Use filesystem.list_directory on the parent to verify the path"],
            },
        };
    }

    [McpServerTool(Name = "filesystem.get_info")]
    [Description(
        "TYPE 2 — Get detailed metadata for a file or directory. No HIL required. " +
        "Returns size, line count (for text files), encoding hint, extension, " +
        "and an agent-readable summary. Use before read_file on large or unknown files.")]
    public async Task<FileResult> GetInfo(
        [Description("Absolute path. Must be under an AllowedRoot.")] string path,
        CancellationToken cancellationToken = default)
    {
        if (!config.IsPathAllowed(path))
            return Err(config.SandboxViolationMessage(path));

        if (!File.Exists(path) && !Directory.Exists(path))
            return Err($"Path not found: '{path}'.");

        try
        {
            if (Directory.Exists(path))
            {
                var di        = new DirectoryInfo(path);
                var fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                var dirCount  = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Length;

                return new FileResult
                {
                    Success    = true,
                    Path       = path,
                    Summary    = $"Directory '{di.Name}': {fileCount} files, {dirCount} sub-dirs, last modified {di.LastWriteTimeUtc:u}.",
                    Structured = new
                    {
                        kind              = "directory",
                        name              = di.Name,
                        full_path         = di.FullName,
                        file_count        = fileCount,
                        directory_count   = dirCount,
                        last_modified_utc = di.LastWriteTimeUtc,
                        created_utc       = di.CreationTimeUtc,
                    },
                    NextActions = ["Use filesystem.list_directory to enumerate contents"],
                };
            }
            else
            {
                var fi       = new FileInfo(path);
                var lang     = DetectLanguage(path);
                var tooLarge = fi.Length > config.MaxFileSizeBytes;

                int? lineCount = null;
                if (!tooLarge && IsTextExtension(path))
                {
                    var lines  = await File.ReadAllLinesAsync(path, cancellationToken);
                    lineCount  = lines.Length;
                }

                return new FileResult
                {
                    Success    = true,
                    Path       = path,
                    Summary    = $"File '{fi.Name}': {fi.Length:N0} bytes" +
                                 (lineCount.HasValue ? $", {lineCount} lines" : "") +
                                 $", {lang}, last modified {fi.LastWriteTimeUtc:u}" +
                                 (tooLarge ? " — TOO LARGE for single read, use fromLine/toLine chunking." : "."),
                    Structured = new
                    {
                        kind              = "file",
                        name              = fi.Name,
                        full_path         = fi.FullName,
                        extension         = fi.Extension,
                        detected_language = lang,
                        size_bytes        = fi.Length,
                        line_count        = lineCount,
                        is_too_large      = tooLarge,
                        max_size_bytes    = config.MaxFileSizeBytes,
                        last_modified_utc = fi.LastWriteTimeUtc,
                        created_utc       = fi.CreationTimeUtc,
                    },
                    NextActions = tooLarge
                        ? ["Use filesystem.read_file with fromLine/toLine to read in chunks"]
                        : ["Use filesystem.read_file to read content"],
                };
            }
        }
        catch (Exception ex)
        {
            _logFault(logger, ex.Message, ex);
            return Err($"GetInfo fault: {ex.Message}");
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static FileResult Err(string message) =>
        new()
        {
            Success  = false,
            Error    = message,
            Summary  = $"Error: {message}",
            NextActions = ["Read the error message, self-correct the path or parameters, and retry"],
        };

    private static string DetectLanguage(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".cs"     => "C#",
            ".csproj" => "MSBuild/XML",
            ".json"   => "JSON",
            ".md"     => "Markdown",
            ".yaml" or ".yml" => "YAML",
            ".xml"    => "XML",
            ".ts"     => "TypeScript",
            ".tsx"    => "TypeScript/React",
            ".js"     => "JavaScript",
            ".py"     => "Python",
            ".sh"     => "Shell",
            ".ps1"    => "PowerShell",
            ".sql"    => "SQL",
            ".html"   => "HTML",
            ".css"    => "CSS",
            ".toml"   => "TOML",
            ".props"  => "MSBuild",
            ".targets" => "MSBuild",
            _         => "plaintext",
        };

    private static bool IsTextExtension(string path) =>
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".csproj", ".json", ".md", ".yaml", ".yml", ".xml", ".ts", ".tsx",
            ".js", ".py", ".sh", ".ps1", ".sql", ".html", ".css", ".toml", ".props",
            ".targets", ".txt", ".log", ".env", ".gitignore", ".dockerignore",
        }.Contains(Path.GetExtension(path));
}

// ── Result contract ───────────────────────────────────────────────────────────

/// <summary>
/// I Am the FileResult. I am the augmented output contract for all filesystem tool
/// calls. I implement the Anthropic Extract + Summarize pattern: agents read
/// .Summary to reason, .Structured to act, and .Lines when they need raw content.
/// .NextActions gives the agent an explicit "what to do next" without hallucination.
/// </summary>
public sealed class FileResult
{
    /// <summary>Boolean gate — check this first before reading any other field.</summary>
    public bool Success { get; init; }

    /// <summary>Absolute path this result refers to. Null on some error paths.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; init; }

    /// <summary>
    /// One-sentence natural language summary the agent can embed directly in its
    /// reasoning chain without re-reading raw output. Always set, even on error.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Typed structured data the agent addresses by field name.
    /// Shape varies by tool — agent should read field names from the JSON keys.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Structured { get; init; }

    /// <summary>
    /// Raw file lines (read_file only). Null for all other tools.
    /// Agent reads this when it needs to modify or analyse content line-by-line.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Lines { get; init; }

    /// <summary>
    /// Error message. Set only when Success=false.
    /// Always contains actionable guidance — the agent should read and self-correct.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    /// <summary>
    /// Agent-readable list of recommended next steps (ANTHROPIC-AGENT-001).
    /// The agent picks the most appropriate action without guessing.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? NextActions { get; init; }
}
