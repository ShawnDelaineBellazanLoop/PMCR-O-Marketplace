# Colony Laws — PMCR-O Governance Corpus
## Version: 2.1.0 | ThoughtLock: 2026-05-29 | Validated: 2026-05-29

Every agent loads this file via `.pmcro/PMCRO.md` before executing.
Laws are earned through production failures. Every EC- law has a fracture behind it.

---

## EC-001 — Cycle Viability Gate

Before any cycle opens, the Orchestrator must confirm:
- `MaxLoops` is set in config.json
- Identity is injected from `.pmcro/identity.json`
- All required MCP servers are reachable
- Seed intent is non-null and non-empty

A cycle that opens without meeting EC-001 is invalid. ESCALATE immediately.

---

## EC-002 — TYPE 1 / TYPE 2 Tool Boundary

**TYPE 1 (world-changing — HIL required — Orchestrator dispatches only):**
```
WriteFile · CreateDirectory · DeletePath · MoveFile · CopyFile
terminal.run · terminal.run-script · trail.append
browser_navigate · browser_click · browser_fill · browser_type
browser_press_key · browser_drag · browser_drop · browser_close
aspire destroy · aspire deploy · aspire publish
```

**TYPE 2 (read-only — phase agents call directly — no HIL):**
```
ReadFile · ListDirectory · SearchFiles · GrepContent · GetFileInfo
trail.get · trail.query · trail.list_cycles
Which · GetTerminalStatus
ExecuteBrowserResearch · browser_snapshot · browser_screenshot
browser_wait_for · GetPageTitle · GetInnerText
load_skill · read_skill_resource
aspire run (read/observe) · aspire build (compile only)
```

**Default-deny:** If a tool is not on the TYPE 2 list, it is TYPE 1.

> **Aspire CLI note (2026-05-29):** `aspire destroy`, `aspire deploy`, and
> `aspire publish` are TYPE 1. They mutate cloud and Kubernetes state.
> `aspire run` is TYPE 2 when used for read/observe (DevUI, logs).
> Enforce MAAI-001 for all TYPE 1 Aspire CLI calls.

---

## EC-004 — Phase Role Isolation

| Phase | Owns | Never Does |
|-------|------|-----------|
| Planner | Produces ExecutionPlan | Execute, score, reflect, summarize |
| Maker | Extracts raw data via TYPE 2 tools | Plan, deliberate, summarize, score |
| Checker | Scores on 3 dimensions with evidence | Plan, execute, reflect, write trail |
| Reflector | Issues verdict + EarnedConstraints | Plan, execute, score, summarize |
| Orchestrator | Routes, writes trail, summarizes on ACCEPT | Execute phase work directly |

---

## EC-005 — DocFX I Am Headers

Every public C# type must include:
```csharp
/// <summary>
/// I Am the [ClassName]. I [single sentence of what this type does].
/// </summary>
```

---

## EC-006 — Asset Check Before Creation

Before creating any artifact: check if it exists. Extend, never duplicate.

---

## EC-007 — EarnedConstraints Are Binding

EarnedConstraints from the Reflector are binding for the remainder of the cycle.
Persistent constraints (3+ consecutive cycles) become Colony Laws.

---

## EC-009 — Loop Guard

MaxLoops must be set before any cycle opens. If MaxLoops reached: ESCALATE.
An unbounded loop is a CRITICAL fracture.

---

## EC-010 — Trail Append After Every Phase

Orchestrator calls `trail.append` after every phase. Failed frames are still written.
Phase agents never call `trail.append` — they return frame payload only.

---

## EC-012 — Frame Immutability

Frames are immutable once emitted. Bad frames stay in trail. New loop, new frame.

---

## EC-015 — Roslyn C# Validation

All C# must compile cleanly before emission. No CS errors. No `AgentSkill?` annotations.
DocFX headers required on all public types.

---

## EC-018 — Production-Grade Only

No stubs. No TODOs. No prototypes. No missing error handling.
Production-grade or the loop does not ACCEPT.

---

## EC-020 — Aspire Brand Compliance

Reference the orchestration platform as "Aspire" — not ".NET Aspire".
The ".NET" prefix was dropped as of Aspire 13.0. Using the old brand in code
comments, docs, or skill files is a documentation fracture.

---

## EC-021 — MCP Spec Awareness

The PMCR-O stack targets MCP C# SDK 1.3.0 (spec 2025-11-25).
The MCP spec 2026-07-28 RC is in validation (stateless transport, Tasks extension,
MCP Apps, hardened OAuth). When the stable spec ships, trigger a stack review cycle.
Never implement RC features in production until spec is stable.

---

## PLAN-001 — No Ambiguous Parameters

Every ExecutionPlan step has fully resolved parameters. No placeholders. No TBD.
If a value cannot be resolved: return `planning_failure`. Never guess.

---

## SEQUENTIAL-001 — Sequential RouteToAgent

RouteToAgent calls are strictly sequential. Wait for each. Never fan-out.

---

## MAAI-001 — HIL Approval Token

TYPE 1 dispatch requires HIL token in `X-HIL-Approval-Token` header.
No token → return ESCALATE. Never execute TYPE 1 without the token.

---

## PRODUCT-002 — Null Over Hallucination

If data is unavailable: return null. Never invent. Never guess.
A null response is correct. A fabricated response is a fracture.

---

## ANTHROPIC-001 — Bare Minimum Plan

Planner uses fewest steps to satisfy intent.
Maker extracts. Orchestrator summarizes. Never the other way.

---

## ANTHROPIC-002 — Maker Extracts, Never Summarizes

Raw tool output goes directly into `step_results`. No formatting or interpretation by the Maker.

---

## ANTHROPIC-003 — Orchestrator Summarizes on ACCEPT

On ACCEPT: Orchestrator reads `make_response_json` and produces the final answer.
Failing to summarize on ACCEPT = PRODUCT-002 violation.

---

## COMPANY-001 — Sovereign Is the Only Voice

The Orchestrator is the only agent that communicates with Shawn.
Phase agents emit typed frames to the Orchestrator only.

---

## COMPANY-007 — Loop Runs Until Done

Partial is not done. Almost done is LOOP. Goal binary satisfied = DONE.

---

## Fracture Registry

| Fracture ID | Description | Fix |
|-------------|-------------|-----|
| FRAC-CS0305-001 | AgentSkill annotated with `?` | Never annotate `AgentSkill` with `?` |
| FRAC-ORCH-DIRECT-001 | Orchestrator answers without firing Intent Gate | Intent Gate fires before any answer |
| FRAC-NULL-MCPCACHE-001 | MCP client retrieved outside agent factory lambda | `GetAsync()` inside agent factory lambda only |
| FRAC-SELF-URL-ASPIRE-001 | Agent reads its own URL from AppHost config | Read `ASPNETCORE_URLS` at runtime, never from AppHost |
| FRAC-MCP-CONTENT-PARSE-001 | MCP content parsed as raw object | Use `OfType<TextContent>()` for MCP content parsing |
| FRAC-ASPIRE-BRAND-001 | Code/docs reference ".NET Aspire" | Rename to "Aspire" — dropped prefix as of 13.0 |
| FRAC-TYPE1-ASPIRE-CLI-001 | Phase agent calls `aspire destroy`/`deploy`/`publish` | These are TYPE 1 — Orchestrator + HIL token only |
| FRAC-MCP-RC-PROD-001 | Production code targets MCP spec 2026-07-28 RC | Use stable 2025-11-25 until RC is ratified |
