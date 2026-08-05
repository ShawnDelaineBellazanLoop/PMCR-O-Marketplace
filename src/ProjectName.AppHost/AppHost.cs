// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AppHost
// File       : AppHost.cs
// Identity   : Aspire orchestration entry point
// ThoughtLock: 2026-05-30
//
// Resource graph:
//
//   [ollama-server]
//     ├── model-orchestrator  (qwen3:8b)
//     ├── model-research      (qwen3:8b)
//     ├── model-reflector     (qwen3:8b)
//     ├── model-validator     (qwen3:8b)
//     ├── model-audit         (qwen3:8b)
//     ├── model-reactive      (qwen3:8b)
//     └── model-vision        (llava:13b)
//
//   [projectname-mcp-filesystem]  ← AllowedRoots, MaxFileSizeBytes, MaxDirectoryDepth
//   [projectname-mcp-terminal]    ← WorkingRoot, CommandTimeoutSeconds, MaxOutputBytes
//   [projectname-mcp-playwright]  ← Headless, AllowedDomains, timeouts
//
//   [projectname-agentservice]    ← MAF agent host (PMCRO loop)
//     WaitFor: all 6 model resources + all 3 MCP servers
//     WithEnvironment: OLLAMA_MODEL_* → ollamaModelParam / ollamaVisionModelParam
//
//   [agent-devui]  ← MAF Agent DevUI, wired to agentservice (Development only)
//
// External Parameters (override via appsettings.json or env vars):
//   Parameters:working-root         → sandbox root for Terminal + Filesystem MCPs
//   Parameters:ollama-model         → model tag for all cognitive roles (default: qwen3:8b)
//   Parameters:ollama-vision-model  → model tag for vision role (default: llava:13b)
// ═══════════════════════════════════════════════════════════════════════════════

using Aspire.Hosting;
using CommunityToolkit.Aspire.Hosting.Ollama;
using System.IO;
using System;

var builder = DistributedApplication.CreateBuilder(args);

// ── 1. EXTERNAL PARAMETERS ────────────────────────────────────────────────────
var ollamaModelParam       = builder.AddParameter("ollama-model");
var ollamaVisionModelParam = builder.AddParameter("ollama-vision-model");

// ── 2. WORKING ROOT ───────────────────────────────────────────────────────────
var workingRoot = builder.Configuration["Parameters:working-root"] ?? @"A:\PMCR-O";
Console.WriteLine($"[BOOT] WorkingRoot: {workingRoot}");

var ollamaModelTag       = builder.Configuration["Parameters:ollama-model"]       ?? "qwen3:8b";
var ollamaVisionModelTag = builder.Configuration["Parameters:ollama-vision-model"] ?? "llava:13b";

// ── 3. OLLAMA SERVER ──────────────────────────────────────────────────────────
var ollama = builder.AddOllama("ollama-server")
    .WithGPUSupport(OllamaGpuVendor.Nvidia)
    .WithDataVolume("ollama-data")
    .WithLifetime(ContainerLifetime.Persistent);

// ── 4. MODEL RESOURCES ────────────────────────────────────────────────────────
var modelOrchestrator = ollama.AddModel("model-orchestrator", ollamaModelTag);
var modelResearch     = ollama.AddModel("model-research",     ollamaModelTag);
var modelReflector    = ollama.AddModel("model-reflector",    ollamaModelTag);
var modelValidator    = ollama.AddModel("model-validator",    ollamaModelTag);
var modelAudit        = ollama.AddModel("model-audit",        ollamaModelTag);
var modelReactive     = ollama.AddModel("model-reactive",     ollamaModelTag);
var modelVision       = ollama.AddModel("model-vision",       ollamaVisionModelTag);

// ── 5. MCP SERVERS ────────────────────────────────────────────────────────────
var terminalMcp = builder
    .AddProject<Projects.ProjectName_Mcp_Terminal>("projectname-mcp-terminal")
    .WithEnvironment("Terminal__WorkingRoot",           workingRoot)
    .WithEnvironment("Terminal__CommandTimeoutSeconds", "60")
    .WithEnvironment("Terminal__MaxOutputBytes",        "65536");

var filesystemMcp = builder
    .AddProject<Projects.ProjectName_Mcp_Filesystem>("projectname-mcp-filesystem")
    .WithEnvironment("Filesystem__AllowedRoots",      workingRoot)
    .WithEnvironment("Filesystem__MaxFileSizeBytes",  "1048576")
    .WithEnvironment("Filesystem__MaxDirectoryDepth", "5");

var playwrightMcp = builder
    .AddProject<Projects.ProjectName_Mcp_Playwright>("projectname-mcp-playwright")
    .WithEnvironment("Playwright__Headless",              "false")
    .WithEnvironment("Playwright__AllowedDomains",        "")
    .WithEnvironment("Playwright__BlockedDomains",        "")
    .WithEnvironment("Playwright__NavigationTimeoutMs",   "30000")
    .WithEnvironment("Playwright__SelectorTimeoutMs",     "10000")
    .WithEnvironment("Playwright__EvaluationTimeoutMs",   "5000")
    .WithEnvironment("Playwright__MaxContentLengthBytes", "131072");

// ── 6. AGENT SERVICE ──────────────────────────────────────────────────────────
// WaitFor all model resources and MCP servers before starting.
// OLLAMA_MODEL_* env vars are injected via parameter resources so model changes
// in appsettings.json flow through without AppHost code edits.
var agentservice = builder
    .AddProject<Projects.ProjectName_AgentService>("projectname-agentservice")
    .WithReference(modelOrchestrator).WaitFor(modelOrchestrator)
    .WithReference(modelResearch).WaitFor(modelResearch)
    .WithReference(modelReflector).WaitFor(modelReflector)
    .WithReference(modelValidator).WaitFor(modelValidator)
    .WithReference(modelAudit).WaitFor(modelAudit)
    .WithReference(modelReactive).WaitFor(modelReactive)
    .WithReference(filesystemMcp).WaitFor(filesystemMcp)
    .WithReference(terminalMcp).WaitFor(terminalMcp)
    .WithReference(playwrightMcp).WaitFor(playwrightMcp)
    .WithEnvironment("OLLAMA_MODEL_ORCHESTRATOR", ollamaModelParam)
    .WithEnvironment("OLLAMA_MODEL_REACTIVE",     ollamaModelParam)
    .WithEnvironment("OLLAMA_MODEL_RESEARCH",     ollamaModelParam)
    .WithEnvironment("OLLAMA_MODEL_REFLECTOR",    ollamaModelParam)
    .WithEnvironment("OLLAMA_MODEL_VALIDATOR",    ollamaModelParam)
    .WithEnvironment("OLLAMA_MODEL_AUDIT",        ollamaModelParam)
    .WithEnvironment("OLLAMA_MODEL_VISION",       ollamaVisionModelParam)
    .WithEnvironment("OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT", "true")
    .WithEnvironment("WORKSPACE_ROOT", workingRoot)
    .WithExternalHttpEndpoints();

// ── 7. DEVUI ──────────────────────────────────────────────────────────────────
// DevUI Triangle:
//   [agent-devui resource] ← AppHost side: registers the frontend resource
//   [AgentService]         ← service side: builder.AddDevUI() + app.MapDevUI() in Program.cs
//
// WithAgentService() wires the DevUI frontend to point at agentservice so the
// chat/loop UI is surfaced in the Aspire dashboard under "agent-devui".
builder.AddDevUI("agent-devui")
    .WithAgentService(agentservice);

builder.Build().Run();
