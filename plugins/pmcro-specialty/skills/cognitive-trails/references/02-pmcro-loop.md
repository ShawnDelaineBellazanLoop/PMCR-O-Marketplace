# Reference: 02 — PMCR-O Loop
# Level 2 — Plan → Make → Check → Reflect → Orchestrate

---

## What PMCR-O Is

PMCR-O is not a MAF feature. It is a cognitive architecture pattern
implemented using MAF workflows and agents. The loop is the product.

```
SEED INTENT
    |
    v
[ORCHESTRATOR] — Intent Gate
    |-- FACTUAL    -> answer directly (no loop)
    |-- ACTIONABLE -> loop is mandatory
              |
              v
         [PLANNER]   -> execution_plan_json
              |
              v
         [MAKER]     -> make_response_json
              |         (raw extraction, TYPE 2 only)
              v
         [CHECKER]   -> checker_frame_json
              |         (3-dimension score with evidence)
              v
         [REFLECTOR] -> verdict: ACCEPT | LOOP | ESCALATE
              |
    +---------+---------+
    |                   |                       |
  ACCEPT               LOOP                 ESCALATE
    |            (EarnedConstraints          (HIL gate)
    v             -> re-enter Planner)
TRAIL WRITTEN
ORCHESTRATOR SUMMARIZES
```

---

## Phase 1: Planner

**Pattern 2 — Deliberative Agent**
Input: `seed_intent` + optional `loopContext` (EarnedConstraints from prior loops)
Output: `execution_plan_json`

The Planner produces the minimum number of steps to satisfy the intent. (ANTHROPIC-001)
Every parameter in every step is fully resolved before emission. (PLAN-001)
If a value cannot be resolved: `planning_failure`. Never a placeholder.

```json
{
  "execution_plan": {
    "cycle_id": "abc-001",
    "loop": 1,
    "intent_summary": "Read the project README and extract the stack versions.",
    "steps": [
      {
        "step_id": 1,
        "tool": "ReadFile",
        "parameters": { "path": "A:/PMCR-O/README.md" },
        "expected_output": "markdown file contents"
      }
    ],
    "planning_status": "ready"
  }
}
```

---

## Phase 2: Maker

**Pattern 1 — Reactive Agent**
Input: `execution_plan_json`
Output: `make_response_json`

The Maker executes each step sequentially using TYPE 2 tools only.
Raw tool output goes directly into `step_results` — no summarizing, no interpretation. (ANTHROPIC-002)
On failure: record the error, stop, return partial response. Do not pivot.

```json
{
  "make_response": {
    "cycle_id": "abc-001",
    "loop": 1,
    "execution_status": "complete",
    "steps_attempted": 1,
    "steps_succeeded": 1,
    "step_results": {
      "1": {
        "tool": "ReadFile",
        "output": "# PMCR-O\n\nStack: MAF 1.7.0...",
        "status": "success"
      }
    }
  }
}
```

---

## Phase 3: Checker

**Pattern 3 — Goal-Oriented Agent**
Input: `execution_plan_json` + `make_response_json`
Output: `checker_frame_json`

Three dimensions. Each requires evidence from step_results — not inference.

```json
{
  "checker_frame": {
    "cycle_id": "abc-001",
    "loop": 1,
    "scores": {
      "completeness":   { "score": 1.0, "evidence": "Step 1 succeeded, output non-null." },
      "correctness":    { "score": 0.9, "evidence": "Content contains version strings matching intent." },
      "law_compliance": { "score": 1.0, "evidence": "ReadFile is TYPE 2. No violations detected." }
    },
    "overall_pass": true,
    "pass_reason": "All thresholds met.",
    "recommended_verdict": "ACCEPT"
  }
}
```

---

## Phase 4: Reflector

**Pattern 4 — Learning Agent**
Input: `checker_frame_json` + trail history
Output: `reflector_output`

The Reflector issues the verdict. It does not summarize — that is the Orchestrator's job on ACCEPT.
EarnedConstraints are specific, actionable, first-person rules derived from what failed.

```json
{
  "reflector_output": {
    "cycle_id": "abc-001",
    "loop": 1,
    "verdict": "ACCEPT",
    "verdict_reason": "All checker dimensions passed. Intent satisfied.",
    "earned_constraints": [],
    "escalation_detail": null
  }
}
```

**On LOOP — EarnedConstraint example:**
```json
{
  "earned_constraints": [
    {
      "id": "EC-EARNED-2026-05-29-001",
      "rule": "I will not pass a path that was not verified to exist at plan time.",
      "trigger": "Step 1 returned FileNotFoundException — path was not verified.",
      "persistence": "cycle"
    }
  ]
}
```

---

## Phase 5: Orchestrator (on ACCEPT)

**Pattern 5 — Hybrid Agent**
The Orchestrator reads `make_response_json` and produces the final summary for Shawn. (ANTHROPIC-003)
It calls `trail.append` to write the cycle summary.
It writes `earned-constraints.json` to `.pmcro/constraints/` if any persist.

---

## Trail Output (on ACCEPT)

```
.pmcro/trails/{cycle_id}/
  L01-01-planner-frame.json     <- deliberation record
  L01-02-maker-frame.json       <- extraction record
  L01-03-checker-frame.json     <- quality record
  L01-04-reflector-frame.json   <- learning record
  cycle-summary.json            <- what was asked, what was done
  earned-constraints.json       <- what was learned this cycle
```

The trail IS the product. A completed trail IS a cognitive asset.

→ Next: See `03-skill-types.md`
