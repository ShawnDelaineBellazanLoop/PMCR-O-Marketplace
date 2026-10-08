# Reference: 01 — MAF Architecture
# Level 1 — The 3-Pillar Architecture and DevUI Triangle

---

## The 3 Pillars

```
+-------------------------------------------------------------+
|               MAF 1.7.0 — Three Pillars                     |
+-------------------+------------------+---------------------+
|  PILLAR 1         |  PILLAR 2        |  PILLAR 3           |
|  Agents           |  Workflows       |  Infrastructure     |
|                   |                  |                     |
|  AIAgent          |  WorkflowBuilder |  DevUI              |
|  Tools            |  Executors       |  MCP (1.3.0)        |
|  Memory           |  Edges           |  Aspire (13.3.4)    |
|  Middleware       |  Checkpointing   |  OpenTelemetry      |
|  Providers        |  HIL Approvals   |  AG-UI / CopilotKit |
+-------------------+------------------+---------------------+
```

---

## Pillar 1 — Agents (dynamic)

Agent = LLM + Instructions + Tools + Memory + Middleware.
The LLM decides which tools to call and in what order. Dynamic topology.

```csharp
// AgentClassSkill — the production pattern
public class PlannerAgentSkill : AgentClassSkill<PlannerAgentSkill>
{
    public override AgentSkill Skill => new AgentSkill  // never AgentSkill?
    {
        Name = "planner",
        Instructions = """
            I Am the Planner. I operate as a Pattern 2 Deliberative Agent.
            I receive a seed intent and produce a minimal ExecutionPlan JSON.
            I do not execute. I do not score. I do not reflect. I plan.
            """,
        Tools = AgentToolSet.Type2Reads
    };
}
```

**FRAC-CS0305-001:** Never annotate `AgentSkill` with `?`. No nullable override.

**When to use an agent:** Task is open-ended, requires tool reasoning, conversational.

---

## Pillar 2 — Workflows (deterministic)

Workflow = developer defines the graph. LLM executes within nodes.
Explicit topology. Auditability. Checkpointing. HIL gates.

```csharp
var pmcroWorkflow = new WorkflowBuilder()
    .AddExecutor("plan",    plannerAgent)
    .AddExecutor("make",    makerAgent)
    .AddExecutor("check",   checkerAgent)
    .AddExecutor("reflect", reflectorAgent)
    .AddEdge("plan", "make")
    .AddEdge("make", "check")
    .AddEdge("check", "reflect")
    .AddConditionalEdge("reflect", result =>
        result.Verdict == "ACCEPT"   ? WorkflowEnd :
        result.Verdict == "LOOP"     ? "plan" : "escalate")
    .WithCheckpointing()
    .WithHumanApproval("escalate")
    .Build();
```

**When to use a workflow:** Well-defined steps, need auditability, multi-agent coordination,
checkpointing, or HIL approval between steps.

---

## Pillar 3 — Infrastructure

**MCP 1.3.0:** Typed, auditable tool contracts. Every tool call is structured.
**DevUI:** Visual workflow graph + streaming events. One line to enable.
**Aspire 13.3.4:** Service mesh. Wires all services, handles service discovery.
**OpenTelemetry:** Every agent action produces GenAI spans. No instrumentation code needed.
**AG-UI:** SSE protocol. Exposes agents to any CopilotKit frontend.

```csharp
// AppHost/Program.cs — wire DevUI in one line
builder.AddAgentFrameworkDevUI(agentService);

// AgentService/Program.cs — expose AG-UI
builder.AddAgentFrameworkHosting()
    .WithAgui()
    .WithOpenAICompatibleRestApi();
```

---

## The DevUI Triangle

```
         DevUI (Visual Graph + Streaming Events)
              /                    \
             /   development loop   \
            /                        \
  Aspire Dashboard          AG-UI / CopilotKit
  (OTel Traces, Metrics)    (SSE Protocol, Prod Chat)
```

- **DevUI** — local dev. Visualizes graph, streams events, lets you chat any agent.
- **Aspire Dashboard** — observability. Distributed traces, GenAI spans, metrics.
- **AG-UI** — production protocol. CopilotKit frontends connect with no custom API wiring.

---

## Agents vs Workflows — Decision Rule

```
Can I write a deterministic function?        → write a function
Well-defined steps + need auditability/HIL   → Workflow
Open-ended / requires tool reasoning         → Agent
Multiple agents + deterministic topology     → Workflow of agents
Multiple agents + dynamic delegation         → Supervisor Agent
```

PMCR-O uses Workflow: steps are well-defined, auditability required, HIL before TYPE 1.

---

## Aspire Service Topology

```
.NET Aspire 13.3.4 AppHost
  AgentService          MCP Filesystem          DevUI
  [orchestrator] ──────> [ReadFile]             [Graph]
  [planner]             [WriteFile]             [Events]
  [maker]               [trail.get]             [Chat]
  [checker]             [trail.append]
  [reflector]

  Ollama (Docker)        Aspire Dashboard
  qwen3:8b               OTel Traces, GenAI spans
```

**Key Aspire patterns:**
- `builder.AddProject<T>()` — register a service
- `.WithReference(other)` — inject service discovery env vars
- **FRAC-SELF-URL-ASPIRE-001:** Never inject self URL from AppHost. Read `ASPNETCORE_URLS` at runtime.

→ Next: See `02-pmcro-loop.md`
