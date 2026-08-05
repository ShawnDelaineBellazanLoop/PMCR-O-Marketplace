// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — MCP.PLAYWRIGHT
// File       : Program.cs
// Identity   : MCP Server Entry Point — Browser Automation Actuator
// Pillar     : 3 — Infrastructure (MCP Server)
// Law Anchor : EC-002, PW-LAW-001, PW-LAW-003, PW-LAW-005, SEQUENTIAL-001
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using ProjectName.Mcp.Playwright.Configuration;
using ProjectName.Mcp.Playwright.Prompts;
using ProjectName.Mcp.Playwright.Resources;
using ProjectName.Mcp.Playwright.Tools;

var builder = Host.CreateApplicationBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
builder.Services
    .AddOptions<PlaywrightConfig>()
    .BindConfiguration("Playwright")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PlaywrightConfig>>().Value);

// ── Playwright session manager (singleton, serial execution PW-LAW-005) ──────
builder.Services.AddSingleton<PlaywrightSessionManager>();

// ── MCP Server ───────────────────────────────────────────────────────────────
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

// ── Playwright install check ──────────────────────────────────────────────────
// Ensures Chromium is installed; exits with guidance if not.
try
{
    var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium", "--with-deps"]);
    if (exitCode != 0)
    {
        Console.Error.WriteLine("[PLAYWRIGHT] Warning: 'playwright install chromium' returned non-zero. Browser may not be available.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[PLAYWRIGHT] Install check failed: {ex.Message}");
}

var app = builder.Build();
await app.RunAsync();
