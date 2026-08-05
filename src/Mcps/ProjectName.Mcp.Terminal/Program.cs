// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.TERMINAL
// File       : Program.cs
// Identity   : Stateless Streamable HTTP MCP Server — the "Hands" of the stack
// Pillar     : 3 — Infrastructure
// Law Anchor : FRAC-MCP-400-001 (Stateless=true required)
//              FRAC-MCP-406-001 (Accept: application/json, text/event-stream)
//              EC-002 (TYPE 1/2 boundary — structural, not runtime-enforced here)
//              EC-018 (no inline package versions)
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.Mcp.Terminal.Configuration;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire ServiceDefaults ────────────────────────────────────────────────────
builder.AddServiceDefaults();

// ── TerminalConfig ────────────────────────────────────────────────────────────
// Binds the "Terminal" config section. WorkingRoot is required (SAFETY-003 Poka-yoke).
builder.Services.AddOptions<TerminalConfig>()
    .BindConfiguration("Terminal")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TerminalConfig>>().Value);

// ── MCP Server — Stateless Streamable HTTP — All Three Pillars ────────────────
// Stateless=true: required by FRAC-MCP-400-001.
// WithToolsFromAssembly     → discovers [McpServerToolType]     (TerminalTools)
// WithResourcesFromAssembly → discovers [McpServerResourceType] (TerminalResources)
// WithPromptsFromAssembly   → discovers [McpServerPromptType]   (TerminalPrompts)
builder.Services
    .AddMcpServer(o =>
    {
        o.ServerInfo = new()
        {
            Name    = "ProjectName.Mcp.Terminal",
            Version = "1.0.0",
        };
        o.ServerInstructions =
            """
            I Am the ProjectName.Mcp.Terminal MCP Server.
            I am the "Hands" of the PMCR-O cognitive stack — Pillar 3 Infrastructure.

            THREE PILLARS:
              Tools     — TYPE 1 (RunCommand, RunScript, KillProcess) require Orchestrator + HIL (MAAI-001)
                          TYPE 2 (GetTerminalStatus, GetEnvironment, Which) any phase agent may call
              Resources — terminal://status | terminal://environment | terminal://config |
                          terminal://skill | terminal://history/{slot} — all TYPE 2, read-only
              Prompts   — terminal-run-command | terminal-debug-failure | terminal-plan-commands

            Read terminal://skill for the full SKILL.md. Read terminal://status before planning.
            TYPE 1 tools require HIL approval in X-HIL-Approval-Token (EC-002, MAAI-001).
            WorkingRoot sandbox is enforced at the server — no path traversal reaches the shell (SAFETY-003).
            """;
    })
    .WithHttpTransport(o => o.Stateless = true)
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

builder.Services.AddHttpClient();

var app = builder.Build();

// ── Aspire health + OTLP endpoints ───────────────────────────────────────────
app.MapDefaultEndpoints();

// ── MCP transport endpoint ────────────────────────────────────────────────────
app.MapMcp("/mcp");

app.MapGet("/", () => Results.Redirect("/mcp")).ExcludeFromDescription();

app.Run();
