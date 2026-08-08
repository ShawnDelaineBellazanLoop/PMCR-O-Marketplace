---
name: terminal-mcp
description: >
  I Am the Terminal MCP SKILL.md. Load me before issuing any terminal command
  in the PMCR-O cognitive stack. I declare the four-slot model, the three MCP
  pillars, the TYPE 1/2 boundary, and the Anthropic ACI/Poka-yoke gates that
  make this server safe to use from an LLM agent context.
license: Proprietary — Tooensure LLC
compatibility: MAF 1.8.0 | MCP C# SDK 1.3.0 | Aspire 13.3.1 | .NET 10 LTS
agentskills_version: "1.0.0"
compatible_tools:
  - claude-code
  - codex-cli
  - gemini-cli
  - github-copilot
  - cursor
  - maf-declarative
metadata:
  author: tooensure
  version: "2.0.0"
  tier: SHARED — Pillar 3 Infrastructure (MCP Server, not an Agent)
  thoughtlock: "2026-05-30"
  pattern: "N/A — Infrastructure, not an executor"
  mcp_primitives: [Tools, Resources, Prompts]
requires: pmcro-framework
---

# I Am the Terminal MCP

I Am the PMCRO.Mcp.Terminal MCP server. I am the "Hands" of the PMCR-O
cognitive stack. I provide shell execution capabilities to Agents that hold
HIL-approved TYPE 1 dispatch authority. I am not an Agent. I have no LLM.
I do not reason. I execute what I am commanded — within the sandbox.

I fully implement all three MCP primitives: Tools, Resources, and Prompts.

---

## Architecture Role

```
Augmented LLM atom (Anthropic design):

  Brain  = Agent (IChatClient + qwen3:8b via OllamaSharp)
  Hands  = PMCRO.Mcp.Terminal ← YOU ARE HERE
  Memory = terminal://resources (status, environment, config, history, skill)

  Agent + Hands + Memory = Augmented LLM
```

This server is Pillar 3 Infrastructure. It sits below all Agents in the stack.
It is discovered by Agents via Aspire service discovery, not hardcoded URLs.

---

## Three MCP Pillars

### Pillar 1 — Tools (execution surface)

TYPE 1 — world-changing — Orchestrator + HIL approval required (EC-002, MAAI-001):

| Tool | Description |
|------|-------------|
| `terminal.run_command` | Execute one shell command in the named slot |
| `terminal.run_script`  | Write a temp file (.ps1/.sh/.py/.cmd) and execute it |
| `terminal.kill`        | Terminate all processes in a slot (idempotent) |

TYPE 2 — read-only — any phase agent may call directly (no HIL):

| Tool | Description |
|------|-------------|
| `terminal.get_status`      | Slot idle/running states |
| `terminal.get_environment` | ENV vars visible to the server |
| `terminal.which`           | Check if a command exists on PATH |

**ACI Poka-yoke gates on all tools:**
- Slot name validated before any shell invocation
- WorkingDir path traversal checked before any shell invocation (`..` blocked)
- Output truncated with explicit marker at `MaxOutputBytes`
- All results returned as typed `TerminalResult` — never raw shell exceptions

### Pillar 2 — Resources (contextual data, TYPE 2, read-only)

Agents read Resources **before** issuing tool calls. Resources are the retrieval
layer that prevents hallucinated tool paths, slot conflicts, and plan failures.

| Resource URI | Type | Consumer | Purpose |
|--------------|------|----------|---------|
| `terminal://status` | Direct | Planner | Confirm slot idle before RunCommand step |
| `terminal://environment` | Direct | Planner | Verify PATH, DOTNET_ROOT, NODE_PATH |
| `terminal://config` | Direct | Planner, Checker | WorkingRoot, timeouts, output limits |
| `terminal://skill` | Direct | Any agent | This SKILL.md — Progressive Disclosure |
| `terminal://history/{slot}` | Template | Checker, Reflector | Last TerminalResult per slot |

**Progressive Disclosure pattern:**
An Agent that has never used this server reads `terminal://skill` first (this file
via MCP). It does not need a pre-loaded skills file — capabilities travel with the
server. This is the PMCR-O runtime-manifest.json pattern applied to MCP Servers.

### Pillar 3 — Prompts (agent-facing templates)

Agents fetch Prompts to get standardized, versioned message scaffolds. Prompts
encode correct Colony Law behaviour by construction (Poka-yoke at prompt level).

| Prompt | Consumer | Purpose |
|--------|----------|---------|
| `terminal-run-command` | Orchestrator | Pre-flight HIL request scaffold with checklist |
| `terminal-debug-failure` | Checker | Failure analysis scaffold for non-zero ExitCode |
| `terminal-plan-commands` | Planner | Convert build intent → sequenced RunCommand steps |

---

## Four-Slot Model

| Slot | Purpose | Timeout |
|------|---------|---------|
| `terminal-1` | General: build, test, dotnet commands | 60s |
| `terminal-2` | Git operations | 60s |
| `terminal-3` | Package managers: dotnet add, npm, pip | 60s |
| `terminal-4` | Long-running: Playwright, scrapers | 900s (15 min) |

**Rule:** One command per slot per `RunCommand` call (ANTHROPIC-001, SEQUENTIAL-001).
Do not chain with `&&` — issue separate `RunCommand` calls instead.
Do not fan-out across slots — SEQUENTIAL-001 enforces serial execution.

---

## Standard Agent Protocol

This is the correct sequence for any Agent using this server:

```
1. DISCOVER (TYPE 2 — no HIL)
   GET terminal://skill          → load this SKILL.md
   GET terminal://status         → confirm required slots are idle
   GET terminal://environment    → verify tool paths (dotnet, git, node)
   GET terminal://config         → note MaxOutputBytes, WorkingRoot

2. PLAN
   Fetch terminal-plan-commands prompt → produce execution_plan_json
   Each step: { slot, command, workingDir } — all fully resolved (PLAN-001)

3. VERIFY (TYPE 2 — no HIL)
   Call terminal.which("{tool}") for each external tool in the plan
   If tool not found: adapt plan before requesting HIL

4. HIL GATE (TYPE 1 — Orchestrator + approval required)
   Fetch terminal-run-command prompt → produce HIL request
   Await approval in X-HIL-Approval-Token header (MAAI-001)

5. EXECUTE (TYPE 1 — Orchestrator dispatches)
   Call terminal.run_command for each step (serial, SEQUENTIAL-001)

6. SCORE (TYPE 2 — no HIL)
   GET terminal://history/{slot}  → read structured TerminalResult
   If ExitCode != 0: fetch terminal-debug-failure prompt → LOOP or ESCALATE

7. TRAIL
   Orchestrator writes trail.append after every phase (EC-010)
```

---

## Transport & Connectivity

```
Endpoint        : POST /mcp
Transport       : Streamable HTTP, Stateless=true (FRAC-MCP-400-001)
Accept header   : application/json, text/event-stream (FRAC-MCP-406-001)
Service name    : terminal-mcp (Aspire service discovery)
Health check    : GET /healthz (via MapDefaultEndpoints)
ServerInfo      : Name=PMCRO.Mcp.Terminal, Version=2.0.0
```

**FRAC-MCP-400-001:** Stateless=true is non-negotiable. Phase agents call via
PostAsJsonAsync with no MCP session handshake. Stateful SSE returns HTTP 400.

**FRAC-MCP-406-001:** Every HttpClient that calls this server must set:
`DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream")`

---

## Law Anchors

| Law | Application |
|-----|-------------|
| EC-002 | TYPE 1/2 boundary — enforced at tool level and at Orchestrator routing |
| MAAI-001 | HIL token required in `X-HIL-Approval-Token` for all TYPE 1 tool calls |
| SAFETY-003 | WorkingRoot sandbox — path traversal blocked before shell invocation |
| EC-005 | All public C# types carry DocFX "I Am" headers |
| EC-015 | All C# compiles cleanly — no CS errors, no `AgentSkill?` annotations |
| EC-018 | Production-grade — no stubs, no TODOs, no missing error handling |
| PLAN-001 | Every RunCommand step in a plan has fully resolved slot/command/workingDir |
| PRODUCT-002 | Structured TerminalResult returned always — never raw exceptions |
| SEQUENTIAL-001 | One command per RunCommand call — no fan-out |
| FRAC-MCP-400-001 | Stateless=true transport — enforced in Program.cs |

---

## ThoughtLock

```json
{
  "thoughtlock": "2026-05-30",
  "version": "2.0.0",
  "mcp_primitives": {
    "tools": ["terminal.run_command", "terminal.run_script", "terminal.kill",
              "terminal.get_status", "terminal.get_environment", "terminal.which"],
    "resources": ["terminal://status", "terminal://environment", "terminal://config",
                  "terminal://skill", "terminal://history/{slot}"],
    "prompts": ["terminal-run-command", "terminal-debug-failure", "terminal-plan-commands"]
  },
  "slots": {
    "terminal-1": "general/build — 60s",
    "terminal-2": "git — 60s",
    "terminal-3": "packages — 60s",
    "terminal-4": "long-running/playwright — 900s"
  },
  "law-anchors": ["EC-002", "MAAI-001", "SAFETY-003", "EC-005", "EC-015",
                  "EC-018", "PLAN-001", "PRODUCT-002", "SEQUENTIAL-001",
                  "FRAC-MCP-400-001", "FRAC-MCP-406-001"]
}
```
