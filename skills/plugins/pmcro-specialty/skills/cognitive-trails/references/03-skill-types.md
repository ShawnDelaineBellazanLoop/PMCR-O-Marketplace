# Reference: 03 — Agent Skill Types
# Level 3 — ORCHESTRATOR, PHASE, COORDINATOR, REACTIVE, SHARED

Every SKILL.md has a `tier` metadata field. The tier determines:
- What tools the agent can call
- What it receives as input / must return as output
- Which Colony Laws govern it

---

## The Five Skill Tiers

### Tier 1 — ORCHESTRATOR

```yaml
metadata:
  tier: ORCHESTRATOR
  pattern: "Pattern 5 — Hybrid Agent"
```

The outer loop controller. Fires the Intent Gate. Routes phases sequentially.
Writes the trail. Summarizes on ACCEPT. The only agent that dispatches TYPE 1 tools.
The only agent that speaks to Shawn.

**Tool allowlist:**
- TYPE 1 (after HIL): WriteFile, CreateDirectory, DeletePath, terminal.run, trail.append
- TYPE 2 (always): trail.get, trail.query, RouteToAgent

**FRAC-ORCH-DIRECT-001:** Intent Gate must fire before any answer.

```csharp
public class OrchestratorAgentSkill : AgentClassSkill<OrchestratorAgentSkill>
{
    public override AgentSkill Skill => new AgentSkill
    {
        Name = "orchestrator",
        Instructions = """
            I Am the Orchestrator. I operate as a Pattern 5 Hybrid Agent.

            ## INTENT GATE — READ THIS FIRST
            FACTUAL (answer directly): definitions, historical facts, math
            ACTIONABLE (route to planner): any task requiring tool use or file changes

            For ACTIONABLE intents: call RouteToAgent("planner", seedIntent) ONCE.
            Do NOT answer directly. Do NOT retry. Wait for each phase response.
            """,
        Tools = AgentToolSet.OrchestratorTools
    };
}
```

---

### Tier 2 — PHASE

```yaml
metadata:
  tier: PHASE
  pattern: "Pattern 2 — Deliberative Agent"  # or 1, 3, 4
```

Inner loop executors. Four canonical phases:

| Phase | Pattern | Input | Output | Never |
|-------|---------|-------|--------|-------|
| Planner | 2 — Deliberative | seed_intent + loopContext | execution_plan_json | execute, score, reflect |
| Maker | 1 — Reactive | execution_plan_json | make_response_json | plan, deliberate, summarize |
| Checker | 3 — Goal-Oriented | plan + make_response | checker_frame_json | plan, execute, reflect |
| Reflector | 4 — Learning | checker_frame + trail | reflector_output | plan, execute, score |

**Phase agents only use TYPE 2 (read-only) tools.**
They never call `trail.append`. They never speak to Shawn.

---

### Tier 3 — COORDINATOR

```yaml
metadata:
  tier: COORDINATOR
  pattern: "Pattern 7 — Multi-Agent System"
```

Optional pre-execution deliberation layer. Convenes all phase agents at a round table
before the Maker runs — for high-stakes intents where a failed cycle is expensive.
Implemented as a MAF workflow of agents running concurrently.

```csharp
var federationBoard = new WorkflowBuilder()
    .AddExecutor("planner-review",   plannerAgent.AsReviewer())
    .AddExecutor("maker-review",     makerAgent.AsReviewer())
    .AddExecutor("checker-review",   checkerAgent.AsReviewer())
    .AddExecutor("reflector-review", reflectorAgent.AsReviewer())
    .WithConcurrent()
    .Build();
```

---

### Tier 4 — REACTIVE

```yaml
metadata:
  tier: REACTIVE
  pattern: "Pattern 1 — Reactive Agent"
```

Fast, deterministic, no memory. Stimulus → Rule → Action. No deliberation.
Use for: webhook handlers, event classifiers, single-step transforms.

---

### Tier 5 — SHARED

```yaml
metadata:
  tier: SHARED
```

Resources, not executors. Loaded by other skills via `load_skill`.
Colony laws, schemas, allowlists, style guides. The governance substrate.

---

## Skill File Anatomy

```yaml
---
name: skill-name
description: >
  I Am [identity]. Load me when [specific triggers].
license: Proprietary — Tooensure LLC
compatibility: MAF 1.7.0 | MCP 1.3.0 | .NET 10 LTS
metadata:
  author: tooensure
  version: "1.0.0"
  tier: PHASE                   # ORCHESTRATOR | PHASE | COORDINATOR | REACTIVE | SHARED
  thoughtlock: "2026-05-29"
  pattern: "Pattern N — Name"
  proven-in: "description of proof context"
requires: pmcro-framework
allowed-tools: >
  ReadFile ListDirectory trail.get trail.query
---

# Agent Name

## Frame Declaration

I Am the [Agent Name]. I operate as a Pattern N [Pattern Name].
[What I do] [What I never do]

## Protocol

[How I reason, step by step]

## Output Contract

[JSON schema]

## ThoughtLock

[JSON anchoring identity at a point in time]
```

---

## The "I Am" Rule

Every agent declares identity in first person at the top of its SKILL.md.

- **"You are the Planner"** → external directive. Fragile. Context can override it.
- **"I Am the Planner"** → self-declaration. Structural. The agent inhabits the frame.

**EC-EARNED-004:** All seed intents and identity framing use first-person only.
`You` is prohibited in identity blocks.

---

## Quick Reference: Which Tier?

| Use Case | Tier | Pattern |
|----------|------|---------|
| Loop controller, routing, TYPE 1 dispatch | ORCHESTRATOR | 5 — Hybrid |
| Produces a plan | PHASE — Planner | 2 — Deliberative |
| Executes plan steps | PHASE — Maker | 1 — Reactive |
| Scores output | PHASE — Checker | 3 — Goal-Oriented |
| Issues verdict + constraints | PHASE — Reflector | 4 — Learning |
| Pre-execution deliberation board | COORDINATOR | 7 — Multi-Agent |
| Fast event handler / classifier | REACTIVE | 1 — Reactive |
| Colony laws, schemas, style guides | SHARED | N/A |

→ Next: See `04-identity-injection.md`
