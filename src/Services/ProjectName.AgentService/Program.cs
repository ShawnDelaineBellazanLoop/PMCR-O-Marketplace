// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Program.cs
// Identity   : MAF agent host — PMCRO loop (Orchestrator→Planner→Maker→Checker→Reflector)
// Law Anchor : OLS-001 (OllamaSharp), EC-004 (no fan-out), FRAC-ORCH-FANOUT-001,
//              EC-MAF-SKILLS-001 (native progressive disclosure — single skills root)
// ThoughtLock: 2026-05-31
//
// MAF AGENT SKILLS — NATIVE PROGRESSIVE DISCLOSURE (EC-MAF-SKILLS-001)
// ─────────────────────────────────────────────────────────────────────
// MAF's AgentSkillsProvider natively implements a 4-stage progressive disclosure loop:
//   Stage 1 — Advertise  (~100 tokens/skill): name + description injected into system prompt
//   Stage 2 — Load       (<5000 tokens):      agent calls load_skill to get full SKILL.md
//   Stage 3 — Resources  (on demand):         agent calls read_skill_resource for references/assets
//   Stage 4 — Scripts    (on demand):         agent calls run_skill_script to execute bundled code
//
// The correct wiring is ONE AgentSkillsProvider (or .UseFileSkills) pointed at the
// skills/ parent directory. MAF discovers every SKILL.md recursively (up to 2 levels),
// injects only names+descriptions at boot, and exposes load_skill for on-demand loading.
//
// The previous pattern of calling .UseFileSkill() per directory bypassed this entirely —
// it loaded each skill as fixed always-on context, negating progressive disclosure and
// bloating qwen3:8b's effective reasoning budget before any task token arrived.
//
// FRACTURE LOG: FRAC-MAF-SKILLS-STATIC-001
//   Root cause : Multiple UseFileSkill() calls per agent loaded MCP skill docs (filesystem-mcp,
//                terminal-mcp, playwright-mcp) as always-on context in addition to pmcro-framework
//                and the agent-specific skill. Combined token cost ~8-10k tokens per request,
//                leaving qwen3:8b with near-zero reasoning budget for actual task work.
//   Resolution : Single .UseFileSkills(skillsRoot) — MAF native progressive disclosure handles all.
//                pmcro-framework, orchestrator-agent, filesystem-mcp etc are all just skills now.
//                The model sees names+descriptions at boot and loads full docs only when needed.
//   Law locked : EC-MAF-SKILLS-001 — Never call UseFileSkill per-directory. Always UseFileSkills
//                on the parent. MAF progressive disclosure is the correct and sufficient mechanism.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using ProjectName.AgentService.Configuration;
using ProjectName.AgentService.Infrastructure;
using ProjectName.AgentService.Services;
using ProjectName.AgentService.Tools;
using ProjectName.AgentService.Skills;
using ProjectName.ServiceDefaults;
using System.Net.Http.Headers;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddOllamaClients();

// ── MCP CLIENTS ───────────────────────────────────────────────────────────────
// RemoveAllResilienceHandlers(): MCP SSE streams are long-lived;
// default retry/circuit-breaker policies break them.
builder.Services.AddHttpClient("mcp-filesystem", c =>
{
    c.BaseAddress = new Uri("http://projectname-mcp-filesystem");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
}).AddServiceDiscovery().RemoveAllResilienceHandlers();

builder.Services.AddHttpClient("mcp-terminal", c =>
{
    c.BaseAddress = new Uri("http://projectname-mcp-terminal");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
}).AddServiceDiscovery().RemoveAllResilienceHandlers();

builder.Services.AddHttpClient("mcp-playwright", c =>
{
    c.BaseAddress = new Uri("http://projectname-mcp-playwright");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
}).AddServiceDiscovery().RemoveAllResilienceHandlers();

// ── MCP TOOL CACHE ────────────────────────────────────────────────────────────
// Registered as singleton so all agents share one instance.
// Tools are stateless AIFunction delegates — safe to share across agents.
builder.Services.AddSingleton<McpToolCache>();

// ── BEACON SCRAPE SERVICE ─────────────────────────────────────────────────────
// Singleton — holds background task handle; scrapes are serial so singleton is safe.
// Depends on McpToolCache (registered above) — no HttpClient needed; Playwright
// is a stdio MCP and must be called via AIFunction delegates, not raw HTTP.
builder.Services.AddSingleton<BeaconScrapeService>();

// ── SKILLS ROOT ───────────────────────────────────────────────────────────────
// Single root — MAF's AgentSkillsProvider discovers every SKILL.md recursively
// (up to 2 levels deep). All skills — pmcro-framework, agent-specific skills,
// MCP server skill docs — live here and are loaded on demand via load_skill.
// EC-MAF-SKILLS-001: never point per-directory. Always the parent.
var skillsRoot = Path.Combine(AppContext.BaseDirectory, "skills");

// ── WORKSPACE ROOT ──────────────────────────────────────────────────────────
// Injected by AppHost from Parameters:working-root (default A:\PMCR-O).
var workspaceRoot = Environment.GetEnvironmentVariable("WORKSPACE_ROOT") ?? @"A:\PMCR-O";

// ── ORCHESTRATOR SYSTEM PROMPT ────────────────────────────────────────────────
// Kept lean. Skills provide domain depth on demand via load_skill.
// The orchestrator does one thing: classify intent and route or act.
var OrchestratorSystemPrompt = $"""
    # Identity
    I am the PMCRO Orchestrator. I act immediately. I do not narrate, verify, or explain before acting.

    # Workspace
    ALL file paths MUST be absolute paths under: {workspaceRoot}
    When the user says "in root" or gives no folder, use: {workspaceRoot}\\<filename>

    # Decision Rule — NO preamble, NO reasoning text before the tool call

    DIRECT — call the tool immediately, no explanation:
      - File operations (create, read, write, list) → use WriteFile, ReadFile, ListDir directly
      - Terminal commands → use RunCommand directly
      - Factual questions → answer directly in text, no tools

    ROUTE — call RouteToAgent("planner", task) for complex multi-step work:
      - Multiple interdependent steps
      - Research + validation + reflection cycles

    # Hard Rules
    - Act FIRST. Never explain what you are about to do.
    - ONE tool call per turn. Never fan-out.
    - After a tool succeeds, confirm to the user in one sentence. Stop.
    - Never fabricate results. Use tools for real data.
    """;

// ── TYPE 1 tool names — blocked for all non-Maker agents ─────────────────────
static bool IsType1Tool(string name) => name is
    // Filesystem
    "WriteFile" or "DeleteFile" or "MoveFile" or
    // Terminal
    "RunCommand" or "RunScript" or "KillProcess" or
    // Playwright
    "Navigate" or "BrowserClick" or "BrowserFill" or
    "TakeScreenshot" or "EvaluateJs" or "CloseSession";

// ── AGENT REGISTRATIONS ───────────────────────────────────────────────────────
void RegisterAgent(string name, string modelKey, AgentToolSet toolSet)
{
    builder.AddAIAgent(name, (sp, key) =>
    {
        var rawChat = sp.GetRequiredKeyedService<IChatClient>(modelKey);

        // ── Tool list ──────────────────────────────────────────────────────────
        var tools = new List<AITool>();

        if (name == "orchestrator")
        {
            // Orchestrator: RouteToAgent + full MCP tool set.
            // EC-002: orchestrator IS the sole TYPE 1 dispatcher — must have
            // WriteFile, RunCommand etc to act directly on simple tasks.
            //
            // FRAC-ORCH-FANOUT-003: Wrap all orchestrator tools with FanOutGuardAIFunction.
            // A single SemaphoreSlim(1,1) is shared across all tools for this agent.
            // The first tool call per turn acquires the semaphore and executes.
            // Any concurrent calls (fired by FunctionInvokingChatClient from a
            // multi-call model response) immediately return a suppression message.
            // The semaphore is auto-released after 500ms so the next turn starts clean.
            // This is the definitive fix — it works regardless of how FunctionInvokingChatClient
            // internally calls its inner chain.
            var fanOutGuard = new SemaphoreSlim(1, 1);
            tools.Add(FanOutGuardAIFunction.Wrap(AIFunctionFactory.Create(OrchestratorTools.RouteToAgent), fanOutGuard));
            // Beacon scrape tools — deterministic C# loop, zero AI tokens per property
            var spCapture = sp;
            tools.Add(FanOutGuardAIFunction.Wrap(AIFunctionFactory.Create(
                [System.ComponentModel.Description("Start (or resume) scraping all 402 vacant properties from Beacon (Ramsey County). Reads csvPath CSV, scrapes each address via Playwright, saves results to outputDir\\beacon-results.json. Crash-safe — restarts resume from last checkpoint. Returns immediately; runs in background.")]
                (string csvPath, string outputDir) => BeaconScrapeTool.ScrapeVacantProperties(csvPath, outputDir, spCapture)), fanOutGuard));
            tools.Add(FanOutGuardAIFunction.Wrap(AIFunctionFactory.Create(BeaconScrapeTool.GetScrapeStatus), fanOutGuard));
            tools.Add(FanOutGuardAIFunction.Wrap(AIFunctionFactory.Create(BeaconScrapeTool.CancelScrape), fanOutGuard));
            var cache = sp.GetRequiredService<McpToolCache>();
            tools.AddRange(cache.GetNativeTools().Cast<AIFunction>().Select(t => FanOutGuardAIFunction.Wrap(t, fanOutGuard)));
        }
        else if (toolSet != AgentToolSet.None)
        {
            var cache = sp.GetRequiredService<McpToolCache>();
            var allTools = cache.GetNativeTools();

            tools.AddRange(toolSet == AgentToolSet.FullMaker
                ? allTools
                : allTools.Where(t => t is not AIFunction f || !IsType1Tool(f.Name)).Cast<AITool>());
        }

        // ── Client pipeline ────────────────────────────────────────────────────
        // Wrap order: rawChat → ToolFilterChatClient → SingleToolCallChatClient
        //             → FunctionInvokingChatClient
        // SingleToolCallChatClient MUST sit inside FunctionInvokingChatClient.

        // load_skill and read_skill_resource are MAF-managed tools surfaced by
        // AgentSkillsProvider. We block them on the orchestrator (it should act
        // or route, not spend turns loading skills mid-dispatch), but allow them
        // for all phase agents — this is the correct progressive disclosure gate.
        // Planner in particular benefits from load_skill to fetch MCP server docs
        // before building file/terminal/browser steps.
        string[] forbiddenTools = name == "orchestrator"
            ? ["load_skill", "read_skill_resource", "run_skill_script"]
            : [];

        IChatClient filteredChat = new ToolFilterChatClient(rawChat, forbiddenTools);

        // Pipeline (outermost → innermost):
        //   Type1LockoutChatClient          ← strips tools after TYPE 1 success
        //     FunctionInvokingChatClient    ← dispatches tool calls in a loop
        //       SingleToolCallChatClient    ← fan-out guard: strips extra tool calls
        //         ToolFilterChatClient      ← blocks forbidden tool names
        //           rawChat (LLM)
        //
        // SingleToolCallChatClient sits INSIDE FunctionInvokingChatClient so it
        // intercepts the raw model response (which may contain N tool calls) and
        // strips all but the first BEFORE FunctionInvoking dispatches them.
        // FRAC-ORCH-FANOUT-001: AllowConcurrentInvocation=false only prevents
        // parallel dispatch; it does NOT drop extra calls. The guard must strip them.
        IChatClient withFanOutGuard = name == "orchestrator"
            ? new SingleToolCallChatClient(filteredChat)
            : filteredChat;

        IChatClient funcInvoking = new FunctionInvokingChatClient(withFanOutGuard)
        {
            AllowConcurrentInvocation = false,
            MaximumIterationsPerRequest = name == "orchestrator" ? 4 : 20
        };

        // Type1LockoutChatClient is outermost — it sees completed tool results
        // and strips Tools from ChatOptions on the next iteration.
        IChatClient chatClient = name == "orchestrator"
            ? new Type1LockoutChatClient(funcInvoking)
            : funcInvoking;

        // ── Skills — MAF Native Progressive Disclosure ─────────────────────────
        // ONE provider, ONE root directory. MAF discovers all SKILL.md files
        // recursively (up to 2 levels), injects name+description at boot (~100
        // tokens/skill), and exposes load_skill for on-demand full-body loading.
        //
        // This replaces the previous per-directory UseFileSkill() calls.
        // All skills (pmcro-framework, orchestrator-agent, filesystem-mcp, etc.)
        // are now first-class skills in the single provider.
        //
        // EC-MAF-SKILLS-001: single root, MAF progressive disclosure, never per-dir.
        var skillsProvider = new AgentSkillsProviderBuilder()
            .UseFileSkill(skillsRoot)
            .UseFileScriptRunner(SubprocessScriptRunner.RunAsync)
            .Build();

        string? agentInstructions = name == "orchestrator" ? OrchestratorSystemPrompt : null;

        return new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = key,
            UseProvidedChatClientAsIs = true,
            ChatOptions = new ChatOptions
            {
                Tools = tools.Count > 0 ? [.. tools] : null,
                Instructions = agentInstructions,
                AdditionalProperties = new AdditionalPropertiesDictionary
                {
                    ["think"]               = false,
                    ["parallel_tool_calls"] = false
                }
            },
            AIContextProviders = [skillsProvider]
        });
    });
}

RegisterAgent("orchestrator", OllamaExtensions.Keys.Orchestrator, AgentToolSet.None);
RegisterAgent("planner",      OllamaExtensions.Keys.Orchestrator, AgentToolSet.Type2Reads);
RegisterAgent("maker",        OllamaExtensions.Keys.Reactive,     AgentToolSet.FullMaker);
RegisterAgent("checker",      OllamaExtensions.Keys.Validator,    AgentToolSet.Type2Reads);
RegisterAgent("reflector",    OllamaExtensions.Keys.Reflector,    AgentToolSet.Type2Reads);

// ── HOST SETUP ────────────────────────────────────────────────────────────────
builder.AddOpenAIResponses();
builder.AddOpenAIConversations();

if (builder.Environment.IsDevelopment())
    builder.AddDevUI();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenAIResponses();
app.MapOpenAIConversations();

if (app.Environment.IsDevelopment())
{
    app.MapDevUI();
    app.MapGrpcReflectionService();
}

app.MapGrpcService<AgentGrpcService>();

app.Lifetime.ApplicationStarted.Register(() =>
{
    AgentServiceLog.Ready(app.Logger, Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "(not set)");
});

await app.RunAsync();
