// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.FILESYSTEM
// File       : Prompts/FilesystemPrompts.cs
// Identity   : MCP Pillar 3 — Prompts (agent loop scaffolds for file operations)
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, EC-004, ANTHROPIC-ACI-001, ANTHROPIC-AGENT-001
// ThoughtLock: 2026-05-30
//
// Anthropic Autonomous Agent Design:
//   Prompts encode the full agent loop, not just a pre-flight checklist.
//   Each prompt scaffold includes:
//     PLAN  — what to read/verify before acting
//     ACT   — which tools to call and in what order
//     OBSERVE — what the structured result fields mean
//     REFLECT — how to score and whether to loop or accept
//   This gives autonomous agents a complete reasoning frame without hallucination.
// ═══════════════════════════════════════════════════════════════════════════════

using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace ProjectName.Mcp.Filesystem.Prompts;

/// <summary>
/// I Am the Filesystem MCP Prompt Provider — Pillar 3 of the three MCP primitives.
/// I expose parameterized agent loop scaffolds for file read, write, and access-debug
/// operations. I encode the Anthropic Extract+Summarize pattern by construction:
/// agents following my scaffolds never need to hallucinate a next step.
/// All prompts are TYPE 2 — any agent may fetch without HIL (EC-002).
/// </summary>
[McpServerPromptType]
public sealed class FilesystemPrompts
{
    [McpServerPrompt(Name = "filesystem-read-plan")]
    [Description(
        "Scaffold a complete agent loop for reading one or more files: " +
        "PLAN (pre-flight checks) → ACT (tool sequence) → OBSERVE (result fields) → REFLECT (scoring). " +
        "Use before any filesystem.read_file dispatch to ensure the agent reads correctly " +
        "and interprets augmented FileResult fields without hallucination.")]
    public static IEnumerable<ChatMessage> ReadPlan(
        [Description("Absolute path or paths to read (comma-separated).")] string paths,
        [Description("What the agent intends to do with the content after reading.")] string intent,
        [Description("True if the file may be large (>100 KB) — enables chunk-read guidance.")] bool maybeLarge = false)
    {
        // Note: filesystem://stat/{path} below is literal agent instruction text, not a C# interpolation.
        // Using $$ raw string so single {path} is treated as literal braces in the output.
        var statNote = "2. Read filesystem://stat/{path} \u2192 get size_bytes, line_count, is_too_large";

        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Filesystem MCP — Read Plan Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, SAFETY-FS-001, ANTHROPIC-AGENT-001

                \u2550\u2550 PLAN (pre-flight \u2014 TYPE 2, no HIL) \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                1. Read filesystem://roots        \u2192 confirm target path is under an AllowedRoot
                {statNote}
                3. If is_too_large=true: plan fromLine/toLine chunked reads (use line_count to slice)
                4. If is_too_large=false: single filesystem.read_file call

                \u2550\u2550 TARGET \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                Paths : {paths}
                Intent: {intent}
                {(maybeLarge ? "\u26a0 maybeLarge=true \u2014 check is_too_large in stat before reading" : "")}

                \u2550\u2550 ACT (tool call sequence) \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                For each path:
                  Step 1 \u2014 filesystem.file_exists(path) \u2192 verify exists=true, kind="file"
                  Step 2 \u2014 filesystem.read_file(path)   \u2192 returns FileResult
                  Step 3 \u2014 Read FileResult fields:
                    .success        \u2192 if false, read .error and self-correct
                    .summary        \u2192 embed directly in reasoning chain ("The file contains X")
                    .structured     \u2192 use detected_language, line_count, size_bytes for context
                    .lines          \u2192 array of file lines \u2014 index from 0
                    .next_actions   \u2192 follow the first applicable action

                \u2550\u2550 OBSERVE (what the result means) \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                FileResult.success=false   \u2192 path wrong or sandbox violation \u2014 read .error, fix path
                FileResult.lines empty     \u2192 file exists but is empty \u2014 note and continue
                FileResult.is_partial_read \u2192 chunk boundary \u2014 issue next read with fromLine offset
                FileResult.detected_language \u2192 use to select correct syntax when modifying

                \u2550\u2550 REFLECT (scoring for PMCR-O Checker) \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                Score COMPLETENESS : Did all target paths return success=true with non-empty lines?
                Score CORRECTNESS  : Does content match intent? (e.g., correct file, correct section)
                Score COMPLIANCE   : Was AllowedRoots verified first? No path traversal attempted?

                Verdict:
                  All 1.0 \u2192 ACCEPT and pass FileResult.lines to the calling agent context
                  Any 0.0 \u2192 LOOP: read .error or .next_actions and retry with corrected parameters
                  Repeated failures (3 loops) \u2192 ESCALATE to HIL
                """),

            new ChatMessage(ChatRole.User,
                $"Plan and execute filesystem read for: {paths} \u2014 intent: {intent}"),
        ];
    }

    [McpServerPrompt(Name = "filesystem-write-scaffold")]
    [Description(
        "Scaffold a complete agent loop for writing a file: " +
        "PLAN → HIL gate → ACT → OBSERVE → REFLECT. " +
        "Encodes TYPE 1 HIL approval requirement, overwrite-safety check, " +
        "and post-write verification by construction (ANTHROPIC-ACI-001).")]
    public static IEnumerable<ChatMessage> WriteScaffold(
        [Description("Absolute path to write.")] string path,
        [Description("What content will be written — describe, don't paste.")] string contentDescription,
        [Description("True if this path may already exist and would be overwritten.")] bool mayOverwrite = false,
        [Description("HIL justification for this write operation (MAAI-001).")] string hilJustification = "")
    {
        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Filesystem MCP — Write Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, MAAI-001, SAFETY-FS-001, ANTHROPIC-AGENT-001

                \u2550\u2550 PLAN (pre-flight \u2014 TYPE 2) \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                1. Read filesystem://roots         \u2192 confirm '{path}' is under an AllowedRoot
                2. filesystem.file_exists('{path}') \u2192 check exists and kind
                   {(mayOverwrite ? "\u26a0 mayOverwrite=true \u2014 if exists=true, confirm HIL approval covers overwrite" : "If exists=true unexpectedly \u2192 PAUSE and re-confirm intent before writing")}
                3. Read filesystem://stat on parent dir \u2192 confirm parent directory exists

                \u2550\u2550 HIL GATE (MAAI-001 \u2014 TYPE 1 required) \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                Target     : {path}
                Content    : {contentDescription}
                Overwrite? : {(mayOverwrite ? "YES \u2014 existing file will be replaced" : "NO \u2014 creating new file")}
                Justification: {(string.IsNullOrWhiteSpace(hilJustification) ? "\u26a0 MISSING \u2014 provide HIL justification before dispatch" : hilJustification)}

                Orchestrator must obtain HIL approval token before calling filesystem.write_file.
                The X-HIL-Approval-Token header must be set on the request (EC-002, MAAI-001).

                \u2550\u2550 ACT (after HIL approval) \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                Call: filesystem.write_file(path='{path}', content=<full content>, encoding='utf-8')

                \u2550\u2550 OBSERVE \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                FileResult.success=true  \u2192 read .structured.bytes_written and .existed_before
                  .summary              \u2192 embed in agent reasoning: "Created/Overwrote X \u2014 N bytes"
                  .existed_before       \u2192 confirm matches your intent (new vs overwrite)
                  .bytes_written        \u2192 sanity check: 0 bytes = empty content bug
                FileResult.success=false \u2192 read .error \u2014 common causes:
                  SANDBOX-VIOLATION     \u2192 path outside AllowedRoots \u2014 fix path
                  Write fault           \u2192 disk full, permission denied \u2014 escalate to HIL

                \u2550\u2550 POST-WRITE VERIFICATION \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                filesystem.read_file('{path}', toLine=10) \u2192 verify first 10 lines match intent
                If lines mismatch \u2192 re-write with corrected content (loop, max 3)

                \u2550\u2550 REFLECT \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                Score COMPLETENESS : bytes_written > 0 and file_exists post-write?
                Score CORRECTNESS  : first 10 lines match expected content?
                Score COMPLIANCE   : HIL token present? Sandbox verified? No path traversal?

                Verdict: All 1.0 \u2192 ACCEPT | Any 0.0 \u2192 LOOP | 3 loops \u2192 ESCALATE
                """),

            new ChatMessage(ChatRole.User,
                $"Write file: {path} \u2014 content: {contentDescription}"),
        ];
    }

    [McpServerPrompt(Name = "filesystem-debug-access")]
    [Description(
        "Scaffold a Checker analysis for filesystem tool failures: " +
        "SANDBOX-VIOLATION, file-not-found, size-exceeded, and permission errors. " +
        "Checker calls this when any FileResult returns success=false.")]
    public static IEnumerable<ChatMessage> DebugAccess(
        [Description("The tool that failed: filesystem.read_file | write_file | delete_file | move_file | list_directory")] string tool,
        [Description("The path that was attempted.")] string path,
        [Description("The error message from FileResult.error.")] string errorMessage,
        [Description("The full FileResult.next_actions array as a comma-separated string.")] string nextActions = "")
    {
        var diagnosis = errorMessage switch
        {
            var e when e.Contains("SANDBOX-VIOLATION", StringComparison.OrdinalIgnoreCase) =>
                "PATH outside AllowedRoots or matches a DeniedPattern. Read filesystem://roots to see valid roots.",
            var e when e.Contains("not found", StringComparison.OrdinalIgnoreCase) =>
                "Path does not exist. Use filesystem.list_directory on the parent to verify the structure.",
            var e when e.Contains("too large", StringComparison.OrdinalIgnoreCase) =>
                "File exceeds MaxFileSizeBytes. Use filesystem.get_info to see size, then read in chunks with fromLine/toLine.",
            var e when e.Contains("directory", StringComparison.OrdinalIgnoreCase) && e.Contains("recursive", StringComparison.OrdinalIgnoreCase) =>
                "Directory delete requires recursive=true. Confirm HIL approval covers recursive delete.",
            var e when e.Contains("fault", StringComparison.OrdinalIgnoreCase) =>
                "Unexpected I/O error \u2014 disk permission or OS-level issue. May require terminal.run_command for diagnostics.",
            _ =>
                "Unknown error. Read filesystem://config to verify limits and filesystem://roots to verify sandbox.",
        };

        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Filesystem MCP — Access Debug Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, SAFETY-FS-001, PRODUCT-002

                You are the Checker. A filesystem tool call failed. Do not hallucinate success.
                PRODUCT-002: null (LOOP) over hallucination. Always.

                \u2550\u2550 Failure Summary \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                Tool   : {tool}
                Path   : {path}
                Error  : {errorMessage}
                Diagnosis: {diagnosis}
                Next actions from result: {(string.IsNullOrWhiteSpace(nextActions) ? "(none provided \u2014 read filesystem://config)" : nextActions)}

                \u2550\u2550 Checker Resolution Protocol \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                SANDBOX-VIOLATION \u2192
                  1. Read filesystem://roots (TYPE 2, no HIL)
                  2. Identify which AllowedRoot covers the intended path
                  3. Reconstruct the absolute path correctly \u2192 LOOP with corrected path

                File not found \u2192
                  1. filesystem.list_directory(parent of '{path}') \u2192 verify actual names
                  2. Note typos or case differences
                  3. Correct the path \u2192 LOOP

                File too large \u2192
                  1. Read filesystem://stat/{path} \u2192 get line_count
                  2. Slice: fromLine=1, toLine=200; increment by 200 per loop
                  3. Assemble chunks in agent context \u2192 ACCEPT when all chunks read

                I/O fault \u2192
                  1. terminal.which('dotnet') to verify environment
                  2. Escalate to HIL if permission denied

                \u2550\u2550 Checker Scoring \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550
                COMPLETENESS: Did the tool produce any usable data? (partial reads count)
                CORRECTNESS : Did the operation match the plan intent?
                COMPLIANCE  : Was the sandbox respected? Was HIL present for TYPE 1?

                Verdict: Issue LOOP with specific corrected parameters, or ESCALATE after 3 loops.
                """),

            new ChatMessage(ChatRole.User,
                $"Debug filesystem failure: tool={tool}, path={path}, error={errorMessage}. Produce verdict."),
        ];
    }
}
