# Reference: 06 — Colony Laws
# Level 6 — Governance Corpus, TYPE 1/2 Boundary, EC- Law Index

---

## What Colony Laws Are

Colony Laws are earned constraints that have promoted to permanent governance rules.
Every EC- law has a fracture behind it — a production failure that made the rule necessary.
They are not guidelines. They are structural. Every agent loads them before executing.

Full corpus: `.pmcro/laws/colony-laws.md`
TYPE 2 allowlist: `.pmcro/laws/type2-allowlist.md`

---

## The TYPE 1 / TYPE 2 Boundary (EC-002)

This is the most critical boundary in the entire system.

**TYPE 1 — world-changing — HIL required — Orchestrator dispatches only:**
```
WriteFile · CreateDirectory · DeletePath · MoveFile · CopyFile
terminal.run · terminal.run-script · trail.append
browser_navigate · browser_click · browser_fill · browser_type
browser_press_key · browser_drag · browser_drop · browser_close
```

**TYPE 2 — read-only — phase agents call directly — no HIL:**
```
ReadFile · ListDirectory · SearchFiles · GrepContent · GetFileInfo
trail.get · trail.query · trail.list_cycles
Which · GetTerminalStatus
ExecuteBrowserResearch · browser_snapshot · browser_screenshot
browser_wait_for · GetPageTitle · GetInnerText
load_skill · read_skill_resource
```

**Default-deny:** If a tool is not explicitly on the TYPE 2 list, it is TYPE 1.

---

## EC- Law Index

| Law | Summary |
|-----|---------|
| **EC-001** | Cycle Viability Gate — MaxLoops set, identity injected, MCP reachable, seed non-null |
| **EC-002** | TYPE 1/2 boundary — HIL required for TYPE 1, default-deny |
| **EC-004** | Phase role isolation — each phase owns exactly one function |
| **EC-005** | DocFX I Am headers on all public C# types |
| **EC-006** | Asset check before creation — extend, never duplicate |
| **EC-007** | EarnedConstraints are binding for the cycle. 3+ cycles → Colony Law |
| **EC-009** | Loop Guard — MaxLoops must be set. Unbounded loop = CRITICAL fracture |
| **EC-010** | Trail append after every phase — Orchestrator only, never phase agents |
| **EC-012** | Frame immutability — frames immutable once emitted |
| **EC-015** | Roslyn C# validation — all C# compiles cleanly before emission |
| **EC-018** | Production-grade only — no stubs, no TODOs, no missing error handling |
| **PLAN-001** | No ambiguous parameters — every plan step fully resolved, no placeholders |
| **SEQUENTIAL-001** | RouteToAgent calls strictly sequential — never fan-out |
| **MAAI-001** | HIL approval token required before TYPE 1 dispatch |
| **PRODUCT-002** | Null over hallucination — never invent, never guess |
| **ANTHROPIC-001** | Bare minimum plan — fewest steps to satisfy intent |
| **ANTHROPIC-002** | Maker extracts, never summarizes — raw output into step_results |
| **ANTHROPIC-003** | Orchestrator summarizes on ACCEPT — never the phases |
| **COMPANY-001** | Orchestrator is the only voice — only agent that speaks to Shawn |
| **COMPANY-007** | Loop runs until done — partial is not done |

---

## Fracture Registry

Every fracture is a production failure that generated a law. Learn from them.

| Fracture ID | What Happened | Fix |
|-------------|---------------|-----|
| FRAC-CS0305-001 | `AgentSkill?` nullable annotation caused cascade compile errors | Never annotate `AgentSkill` with `?` |
| FRAC-ORCH-DIRECT-001 | Orchestrator answered actionable intent directly from training | Intent Gate fires before any answer, always |
| FRAC-NULL-MCPCACHE-001 | `GetAsync()` called at builder configuration time, BaseAddress null | `GetAsync()` inside agent factory lambda only |
| FRAC-SELF-URL-ASPIRE-001 | Self URL injected from AppHost before Kestrel bound | Read `ASPNETCORE_URLS` at runtime, never from AppHost |
| FRAC-MCP-CONTENT-PARSE-001 | `ContentBlock` cast failed — not inheritance in MCP SDK 1.3.0 | Use `OfType<TextContent>()` discriminated union |
| EARNED-2026-05-26-001 | Agent called non-existent "docker mcp tool call playwright" | Validate tool names against registered MCP tools at startup |
| EARNED-2026-05-26-002 | MCP Gateway token hardcoded in plan | Tokens captured at runtime, never hardcoded in frames |

---

## HIL Gate Protocol (MAAI-001)

The Human-in-the-Loop gate is structural — it cannot be bypassed by prompt.

```
1. Reflector issues ESCALATE verdict
2. Orchestrator holds execution
3. Orchestrator emits escalation_detail to Shawn via OrchestrationApi
4. Shawn reviews: APPROVE or REJECT
5. APPROVE → Shawn sends HIL approval token (X-HIL-Approval-Token header)
6. Orchestrator validates token
7. Orchestrator dispatches TYPE 1 tool with the token
8. Orchestrator writes trail.append with HIL approval record
```

No token = no TYPE 1 dispatch. This is the law. It cannot be reasoned around.

---

## EarnedConstraint Lifecycle

```
1. Reflector issues EarnedConstraint on LOOP
2. Constraint is injected into loopContext for the next Planner run
3. Planner acknowledges constraint in execution_plan.earned_constraints_applied
4. If constraint appears in 3+ consecutive cycles:
   a. Reflector flags it as persistent
   b. Orchestrator writes to .pmcro/constraints/{id}.json
   c. Law is added to colony-laws.md with next available EC- number
   d. All subsequent agents load it as governance, not just loop context
```

---

## Adding a New Law

1. Document the fracture that earned it (what failed, exactly)
2. Write the law in first-person, actionable form: "I will not..." or "I must..."
3. Assign next available EC- number
4. Add to `.pmcro/laws/colony-laws.md`
5. Update the EC- index in this file
6. Update `skill-delta.md` for any skills that need updating
7. ThoughtLock the update with date

Laws are append-only. Existing laws are never edited — only clarified via new addenda.
