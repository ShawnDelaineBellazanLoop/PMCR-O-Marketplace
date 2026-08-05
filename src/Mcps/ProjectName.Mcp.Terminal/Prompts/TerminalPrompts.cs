// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.TERMINAL
// File       : Prompts/TerminalPrompts.cs
// Identity   : MCP Pillar 3 — Prompts (agent-facing structured prompt templates)
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, EC-004, EC-005, COMPANY-001, ANTHROPIC-ACI-001
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace ProjectName.Mcp.Terminal.Prompts;

/// <summary>
/// I Am the Terminal MCP Prompt Provider. I am Pillar 3 of the three MCP
/// primitives for ProjectName.Mcp.Terminal. I expose parameterized, versioned prompt
/// templates that Agents fetch before performing terminal operations.
/// I am TYPE 2 — any phase agent may fetch me without HIL (EC-002).
/// </summary>
[McpServerPromptType]
public sealed class TerminalPrompts
{
    [McpServerPrompt(Name = "terminal-run-command")]
    [Description(
        "Scaffold a validated RunCommand HIL request for the Orchestrator. " +
        "Enforces correct slot selection, sandbox boundary, and single-command-per-call rule. " +
        "Orchestrator calls this after HIL approval, before dispatching terminal.run_command.")]
    public static IEnumerable<ChatMessage> RunCommandScaffold(
        [Description("The shell command to execute. One command only — no && chaining.")] string command,
        [Description("Target slot: terminal-1 (build/test), terminal-2 (git), terminal-3 (packages), terminal-4 (long-running/Playwright).")] string slot,
        [Description("Working directory relative to WorkingRoot. Empty = WorkingRoot. Must not start with '..'.")] string workingDir = "",
        [Description("Reason this command must run — the HIL justification. Required by MAAI-001.")] string hilJustification = "")
    {
        var slotPurpose = slot switch
        {
            "terminal-1" => "General purpose: build, test, dotnet commands (60s timeout)",
            "terminal-2" => "Git operations (60s timeout)",
            "terminal-3" => "Package managers: dotnet add, npm, pip (60s timeout)",
            "terminal-4" => "Long-running: Playwright, scrapers (900s timeout)",
            _            => $"⚠ UNKNOWN SLOT: '{slot}' — valid slots are terminal-1 through terminal-4",
        };

        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Terminal MCP — RunCommand Pre-flight Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, MAAI-001, SAFETY-003

                You are the Orchestrator. You have received HIL approval to dispatch a
                terminal.run_command TYPE 1 call. Before dispatching, confirm all fields below.

                ─── Command Parameters ───────────────────────────────────────────────────
                Command     : {(string.IsNullOrWhiteSpace(command) ? "⚠ MISSING — required" : command)}
                Slot        : {(string.IsNullOrWhiteSpace(slot) ? "⚠ MISSING — required" : slot)}
                Slot purpose: {slotPurpose}
                WorkingDir  : {(string.IsNullOrWhiteSpace(workingDir) ? "(empty = WorkingRoot)" : workingDir)}

                ─── HIL Gate (MAAI-001) ─────────────────────────────────────────────────
                Justification: {(string.IsNullOrWhiteSpace(hilJustification) ? "⚠ MISSING — MAAI-001 requires justification in HIL token" : hilJustification)}

                ─── Pre-flight Checklist (Poka-yoke, ANTHROPIC-ACI-001) ─────────────────
                □ One command only — no && chaining across this call
                □ WorkingDir does not start with '..' (sandbox enforced by server)
                □ Slot is correct for this command type (see purpose above)
                □ terminal://status shows '{slot}' is idle — not already running
                □ terminal.which("{command.Split(' ')[0]}") confirms tool is on PATH
                □ HIL approval token is present in X-HIL-Approval-Token header

                ─── After Dispatch ───────────────────────────────────────────────────────
                Read terminal://history/{slot} to get the structured TerminalResult.
                Pass TerminalResult.ExitCode, .Stdout, .Stderr to the Checker phase.
                If ExitCode != 0: route to terminal-debug-failure prompt before scoring.
                """),

            new ChatMessage(ChatRole.User,
                $"Dispatch terminal.run_command — slot: {slot} — command: {command}"),
        ];
    }

    [McpServerPrompt(Name = "terminal-debug-failure")]
    [Description(
        "Structure Checker analysis of a failed terminal command (ExitCode != 0 or Success=false). " +
        "Prevents hallucinating a success interpretation. " +
        "Checker calls this when terminal://history/{slot} shows a failure result.")]
    public static IEnumerable<ChatMessage> DebugFailure(
        [Description("The slot that ran the failed command.")] string slot,
        [Description("The command that failed.")] string command,
        [Description("Exit code from the TerminalResult. Should be non-zero.")] int exitCode,
        [Description("Stdout from the TerminalResult.")] string stdout = "",
        [Description("Stderr from the TerminalResult. Often contains the error.")] string stderr = "")
    {
        var severity = exitCode switch
        {
            1   => "Standard failure (exit 1) — command ran but reported an error",
            2   => "Misuse (exit 2) — incorrect command syntax or missing arguments",
            126 => "Permission denied — command found but not executable",
            127 => "Command not found — tool may not be on PATH (call terminal.which to verify)",
            130 => "Interrupted by signal (Ctrl+C or kill) — may be a timeout",
            _   => $"Exit code {exitCode} — non-standard; check Stderr for details",
        };

        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Terminal MCP — Failure Debug Scaffold
                ThoughtLock: 2026-05-30 | Law: EC-002, PRODUCT-002, COMPANY-007

                You are the Checker. A terminal command failed. Your job is to produce
                an accurate checker_frame_json — not to guess that it succeeded.
                PRODUCT-002: null (or LOOP) over hallucination. Always.

                ─── Failure Summary ────────────────────────────────────────────────────
                Slot    : {slot}
                Command : {command}
                ExitCode: {exitCode}
                Severity: {severity}

                ─── Output ────────────────────────────────────────────────────────────
                STDOUT:
                {(string.IsNullOrWhiteSpace(stdout) ? "(empty)" : stdout)}

                STDERR:
                {(string.IsNullOrWhiteSpace(stderr) ? "(empty — check ExitCode severity above)" : stderr)}

                ─── Checker Analysis Protocol ──────────────────────────────────────────
                Score on three dimensions (0.0 – 1.0):

                1. COMPLETENESS — Did the command produce any useful output?
                2. CORRECTNESS  — Did the command do what the plan intended? ExitCode != 0 → 0.0
                3. LAW COMPLIANCE — Was the tool call dispatched correctly? (slot, sandbox, HIL)

                ─── Verdict Guidance ───────────────────────────────────────────────────
                Exit code 127 (not found) → add terminal.which step → LOOP
                Exit code 2 (syntax)      → fix command parameters  → LOOP
                Exit code 1 (error)       → analyse stderr, propose fix → LOOP
                Repeated failures (loop 3) → ESCALATE to HIL for human review
                """),

            new ChatMessage(ChatRole.User,
                $"Analyse failure: slot={slot}, command={command}, exitCode={exitCode}. " +
                $"Produce checker_frame_json with verdict LOOP or ESCALATE."),
        ];
    }

    [McpServerPrompt(Name = "terminal-plan-commands")]
    [Description(
        "Help Planner convert a build/operation intent into a sequenced RunCommand step list. " +
        "Encodes ANTHROPIC-001 (bare minimum plan) and SEQUENTIAL-001 (no fan-out) by construction.")]
    public static IEnumerable<ChatMessage> PlanCommands(
        [Description("The seed intent or operation to plan.")] string intent,
        [Description("Working root path for sandbox context, e.g. 'A:\\PMCR-O'.")] string workingRoot = "",
        [Description("Whether git operations are needed (routes relevant steps to terminal-2).")] bool includeGit = false,
        [Description("Whether package management is needed (routes relevant steps to terminal-3).")] bool includePackages = false)
    {
        var slotGuide = $"""
            Slot selection guide (PMCR-O four-slot model):
              terminal-1 — dotnet restore / build / test / publish / run | general shell | 60s
              terminal-2 — git clone / pull / commit / push / status     | git only      | 60s{(includeGit ? " ← USE THIS for git steps" : "")}
              terminal-3 — dotnet add | npm install | pip install          | packages only | 60s{(includePackages ? " ← USE THIS for package steps" : "")}
              terminal-4 — playwright install | long dotnet test runs      | 15-min timeout (900s)
            """;

        return
        [
            new ChatMessage(ChatRole.System,
                $"""
                PMCR-O Terminal MCP — Command Planning Scaffold
                ThoughtLock: 2026-05-30 | Law: ANTHROPIC-001, SEQUENTIAL-001, EC-002

                You are the Planner. Produce an execution_plan_json for the terminal
                operation intent. Apply these rules by construction:

                ANTHROPIC-001 (Bare Minimum Plan): fewest steps that satisfy the intent.
                SEQUENTIAL-001 (No fan-out): steps execute one at a time, never concurrent.
                SAFETY-003 (Sandbox): workingDir is always relative to WorkingRoot, never '..'.

                {slotGuide}

                WorkingRoot: {(string.IsNullOrWhiteSpace(workingRoot) ? "(read from terminal://config)" : workingRoot)}

                ─── Plan Output Format ──────────────────────────────────────────────────
                Step N — slot: terminal-X | command: <exact shell command> | workingDir: <relative path>

                After listing steps, add:
                  Pre-flight reads: list which terminal:// resources the Maker must read first
                  HIL gate: confirm all RunCommand steps are TYPE 1 and require Orchestrator dispatch
                """),

            new ChatMessage(ChatRole.User,
                $"Plan terminal operations for: {intent}"),

            new ChatMessage(ChatRole.Assistant,
                $"""
                I'll plan the terminal operations for: "{intent}"

                Pre-flight resource reads (TYPE 2, before any RunCommand):
                  1. Read terminal://status — confirm all required slots are idle
                  2. Read terminal://environment — verify tool paths (dotnet, git, node)
                  3. Read terminal://config — note MaxOutputBytes for output-heavy steps

                Planned steps (SEQUENTIAL-001 — execute one at a time):
                """),
        ];
    }
}
