// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.FILESYSTEM
// File       : Program.cs
// Identity   : Filesystem Actuator Boot Sequence
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : FRAC-MCP-400-001 (Stateless=true), FRAC-MCP-406-001 (Accept header),
//              SAFETY-FS-001 (AllowedRoots), EC-002 (TYPE 1/2), EC-018 (no inline versions)
// ThoughtLock: 2026-05-30
//
// Transport note (FRAC-MCP-400-001):
//   Stateless=true — callers use PostAsJsonAsync with no MCP session handshake.
//   Phase services must send: Accept: application/json, text/event-stream (FRAC-MCP-406-001).
//
// Three-pillar registration:
//   WithToolsFromAssembly()     → discovers [McpServerToolType]     (FilesystemTools)
//   WithResourcesFromAssembly() → discovers [McpServerResourceType] (FilesystemResources)
//   WithPromptsFromAssembly()   → discovers [McpServerPromptType]   (FilesystemPrompts)
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.Mcp.Filesystem.Configuration;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire ServiceDefaults (OTLP, health checks, service discovery) ───────────
builder.AddServiceDefaults();

// ── FilesystemConfig — validates AllowedRoots on startup (Poka-yoke, SAFETY-FS-001) ─
builder.Services.AddOptions<FilesystemConfig>()
    .BindConfiguration("Filesystem")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FilesystemConfig>>().Value);

// ── MCP Server — Stateless Streamable HTTP — All Three Pillars ────────────────
builder.Services
    .AddMcpServer(o =>
    {
        o.ServerInfo = new()
        {
            Name    = "ProjectName.Mcp.Filesystem",
            Version = "1.0.0",
        };
        o.ServerInstructions =
            """
            I Am the ProjectName.Mcp.Filesystem MCP Server.
            I am the "File I/O Hands" of the PMCR-O cognitive stack — Pillar 3 Infrastructure.

            THREE PILLARS:
              Tools     — TYPE 1 (read_file, write_file, delete_file, move_file) require Orchestrator + HIL (MAAI-001)
                          TYPE 2 (list_directory, file_exists, get_info) any phase agent may call
              Resources — filesystem://roots | filesystem://config | filesystem://skill |
                          filesystem://stat/{path} — all TYPE 2, read-only
              Prompts   — filesystem-read-plan | filesystem-write-scaffold | filesystem-debug-access

            AGENT PROTOCOL (Anthropic Autonomous Agent Design):
              1. Read filesystem://roots to know the AllowedRoots sandbox boundary
              2. Read filesystem://config to know MaxFileSizeBytes, MaxListEntries, MaxRecursionDepth
              3. Use filesystem.file_exists before reading uncertain paths (TYPE 2, no HIL)
              4. Read filesystem://stat/{path} before reading large files — check is_too_large
              5. All FileResult returns include .summary (reason), .structured (act), .next_actions (navigate)

            Every tool return is augmented with .summary + .structured + .next_actions.
            The agent NEVER needs to re-read raw output to understand what happened.
            Sandbox enforced at FilesystemConfig.IsPathAllowed() — single choke-point (SAFETY-FS-001).
            """;
    })
    .WithHttpTransport(o => o.Stateless = true)
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

var app = builder.Build();

// ── Aspire health + OTLP ─────────────────────────────────────────────────────
app.MapDefaultEndpoints();

// ── MCP transport endpoint ────────────────────────────────────────────────────
app.MapMcp("/mcp");

app.MapGet("/", () => Results.Redirect("/mcp")).ExcludeFromDescription();

app.Run();
