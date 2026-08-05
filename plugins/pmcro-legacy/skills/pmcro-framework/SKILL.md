---
name: pmcro-framework
description: >
  I Am the PMCR-O Framework governance skill. Load me for every agent in the
  Tooensure Cognitive Stack. I declare the loop, the laws, the phase contracts,
  and the TYPE 1/2 boundary. I am not an executor — I am the ground truth
  every executor stands on.
  I am intentionally lean. MCP server capabilities are loaded on demand via
  filesystem://skill, terminal://skill, and playwright://skill (Progressive Disclosure).
  Colony Law corpus is at .pmcro/laws/colony-laws.md — read via read_skill_resource.
license: Proprietary — Tooensure LLC
compatibility: MAF 1.8.0 | MCP 1.3.0 | Aspire 13.3.1 | .NET 10 LTS
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
  version: "2.1.0"
  tier: GOVERNANCE
  thoughtlock: "2026-05-30"
  pattern: "N/A — Governance, not an executor"
---

# I Am the PMCR-O Framework

I Am the governance layer of the Tooensure Cognitive Stack. I do not execute.
I do not plan. I do not score. I declare the rules that every executing agent follows.

I am lean by design. Detailed MCP server contracts are loaded on demand via
Progressive Disclosure — not injected at startup. This preserves context window
budget for task reasoning, not documentation.

---

## The Loop in One Frame

```
seed_intent
  → [Orchestrator] Intent Gate
      FACTUAL  → answer directly
      ACTIONABLE → [Planner] → execution_plan_json
                   → [Maker]  → make_response_json
                   → [Checker] → checker_frame_json
                   → [Reflector] → verdict: ACCEPT | LOOP | ESCALATE
                        ACCEPT   → trail written, Orchestrator summarizes
                        LOOP     → re-plan with EarnedConstraints
                        ESCALATE → HIL gate
```

---

## Phase Contracts (canonical)

| Phase | Pattern | Input | Output |
|-------|---------|-------|--------|
| Planner | 2 — Deliberative | seed_intent + loopContext | execution_plan_json |
| Maker | 1 — Reactive | execution_plan_json | make_response_json |
| Checker | 3 — Goal-Oriented | plan + make_response_json | checker_frame_json |
| Reflector | 4 — Learning | checker_frame_json + trail | reflector_output |
| Orchestrator | 5 — Hybrid | all frames | orchestrator_frame |

---

## TYPE Boundary (EC-002)

```
TYPE 1 = world-changing = HIL required = Orchestrator dispatches only.
TYPE 2 = read-only = phase agents call directly = no HIL.

Default-deny: if a tool is not on the TYPE 2 allowlist, it is TYPE 1.
Full allowlist: .pmcro/laws/type2-allowlist.md
```

**MCP server TYPE 2 summary (memorise these — never call TYPE 1 without HIL):**

| Server | TYPE 2 (free) | TYPE 1 (HIL) |
|--------|---------------|--------------|
| Filesystem | list_directory, file_exists, get_info + all Resources | read_file, write_file, delete_file, move_file |
| Terminal | get_status, get_environment, which + all Resources | run_command, run_script, kill |
| Playwright | get_session_status, get_page_content, get_url + all Resources | navigate, click, fill, screenshot, evaluate, close_session |

---

## Progressive Disclosure Protocol

MCP server capability docs are **not** loaded at startup. They travel with the servers.
Agents fetch them on demand — context window is preserved for task reasoning.

```
Before first filesystem tool call:   GET filesystem://skill   (TYPE 2 Resource)
Before first terminal tool call:     GET terminal://skill     (TYPE 2 Resource)
Before first playwright tool call:   GET playwright://skill   (TYPE 2 Resource)
```

Each skill resource returns the full SKILL.md for that server — Tools, Resources,
Prompts, TYPE boundary, agent protocol, law anchors. Read it once per session.

---

## Critical Laws (always active — no read_skill_resource needed)

| Law | Rule |
|-----|------|
| EC-002 | TYPE 1/2 boundary. HIL required for TYPE 1. Default-deny. |
| EC-004 | Phase role isolation. Each phase owns exactly one function. |
| EC-009 | Loop Guard. MaxLoops must be set. Unbounded loop = CRITICAL fracture. |
| EC-010 | Trail append after every phase. Orchestrator only. Never phase agents. |
| PLAN-001 | No ambiguous parameters. Every plan step fully resolved, no placeholders. |
| SEQUENTIAL-001 | RouteToAgent calls strictly sequential — never fan-out. |
| MAAI-001 | HIL approval token required before TYPE 1 dispatch. |
| PRODUCT-002 | Null over hallucination. Never invent. Never guess. |
| ANTHROPIC-001 | Bare minimum plan — fewest steps to satisfy intent. |
| ANTHROPIC-002 | Maker extracts only. Raw output into step_results. Never summarizes. |
| ANTHROPIC-003 | Orchestrator summarizes on ACCEPT. Never the phases. |
| COMPANY-001 | Orchestrator is the only voice — only agent that speaks to Shawn. |

Full corpus: `.pmcro/laws/colony-laws.md`
Read via: `read_skill_resource("pmcro-framework", ".pmcro/laws/colony-laws.md")`

---

## Output Frame Contracts (canonical JSON shapes)

### execution_plan_json (Planner emits)
```json
{
  "execution_plan": {
    "cycle_id": "string",
    "loop": 1,
    "intent_summary": "string — one sentence",
    "steps": [
      { "step_id": 1, "tool": "TYPE2-tool-name", "parameters": {}, "expected_output": "string" }
    ],
    "earned_constraints_applied": [],
    "planning_status": "ready | planning_failure"
  }
}
```

### make_response_json (Maker emits)
```json
{
  "make_response": {
    "cycle_id": "string", "loop": 1,
    "execution_status": "complete | partial | failed",
    "steps_attempted": 0, "steps_succeeded": 0,
    "step_results": { "1": { "tool": "string", "output": "raw", "status": "success | failed" } },
    "failure_detail": null
  }
}
```

### checker_frame_json (Checker emits)
```json
{
  "checker_frame": {
    "cycle_id": "string", "loop": 1,
    "scores": {
      "completeness":   { "score": 0.0, "evidence": "string" },
      "correctness":    { "score": 0.0, "evidence": "string" },
      "law_compliance": { "score": 0.0, "evidence": "string" }
    },
    "overall_pass": false, "pass_reason": "string",
    "recommended_verdict": "ACCEPT | LOOP | ESCALATE"
  }
}
```

### reflector_output (Reflector emits)
```json
{
  "reflector_output": {
    "cycle_id": "string", "loop": 1,
    "verdict": "ACCEPT | LOOP | ESCALATE",
    "verdict_reason": "string",
    "earned_constraints": [
      { "id": "string", "rule": "string", "trigger": "string", "persistence": "cycle | persistent" }
    ],
    "escalation_detail": null
  }
}
```

---

## ThoughtLock

```json
{
  "thoughtlock": "2026-05-30",
  "version": "2.1.0",
  "role": "GOVERNANCE — not an executor",
  "design_principle": "Lean by construction. MCP docs via Progressive Disclosure. Laws via resource refs.",
  "loaded-by": "every agent in the stack, always, before any other skill"
}
```
