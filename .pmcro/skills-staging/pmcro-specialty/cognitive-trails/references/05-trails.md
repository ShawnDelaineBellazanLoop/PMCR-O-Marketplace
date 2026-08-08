# Reference: 05 — Trails
# Level 5 — TrailFrames, Cognitive Assets, The Product Model

---

## What a Trail Is

A Trail is the complete, structured record of how a specific identity
resolved a specific intent across one or more PMCR-O loops.

The Trail is not a log. It is not a debug artifact.
**The Trail IS the product.** A completed Trail IS a cognitive asset.
A compressed Trail IS a new SKILL.md — ready for distribution.

---

## Trail Structure

```
.pmcro/trails/{cycle_id}/
  L{loop:D2}-01-planner-frame.json    <- deliberation record
  L{loop:D2}-02-maker-frame.json      <- extraction record
  L{loop:D2}-03-checker-frame.json    <- quality record
  L{loop:D2}-04-reflector-frame.json  <- learning record
  cycle-summary.json                   <- what was asked, what was done
  earned-constraints.json              <- what was learned this cycle
  skill-delta.md                       <- what should change in the skills
```

A Trail with 3 loops has 12 frame files plus the summary files.
Every loop is numbered. Every frame is immutable (EC-012). Bad frames stay — new loop, new frame.

---

## The Four Frame Types

### PlannerFrame (deliberation record)

```json
{
  "planner_frame": {
    "cycle_id": "string",
    "loop": 1,
    "seed_intent": "string",
    "earned_constraints_applied": ["string"],
    "execution_plan": { ... },
    "planning_status": "ready | planning_failure",
    "timestamp": "ISO-8601"
  }
}
```

### MakerFrame (extraction record)

```json
{
  "maker_frame": {
    "cycle_id": "string",
    "loop": 1,
    "execution_status": "complete | partial | failed",
    "steps_attempted": 0,
    "steps_succeeded": 0,
    "step_results": { "1": { "tool": "string", "output": "raw", "status": "success" } },
    "failure_detail": null,
    "timestamp": "ISO-8601"
  }
}
```

### CheckerFrame (quality record)

```json
{
  "checker_frame": {
    "cycle_id": "string",
    "loop": 1,
    "scores": {
      "completeness":   { "score": 0.0, "evidence": "string" },
      "correctness":    { "score": 0.0, "evidence": "string" },
      "law_compliance": { "score": 0.0, "evidence": "string" }
    },
    "overall_pass": false,
    "pass_reason": "string",
    "recommended_verdict": "ACCEPT | LOOP | ESCALATE",
    "timestamp": "ISO-8601"
  }
}
```

### ReflectorFrame (learning record)

```json
{
  "reflector_frame": {
    "cycle_id": "string",
    "loop": 1,
    "verdict": "ACCEPT | LOOP | ESCALATE",
    "verdict_reason": "string",
    "earned_constraints": [
      {
        "id": "EC-EARNED-2026-05-29-001",
        "rule": "string",
        "trigger": "string",
        "persistence": "cycle | persistent"
      }
    ],
    "escalation_detail": null,
    "timestamp": "ISO-8601"
  }
}
```

---

## cycle-summary.json

Written by the Orchestrator on ACCEPT. The human-readable record of the cycle.

```json
{
  "cycle_summary": {
    "cycle_id": "string",
    "seed_intent": "string",
    "loops_completed": 1,
    "verdict": "ACCEPT",
    "final_summary": "string — Orchestrator's summary of what was accomplished",
    "trail_path": ".pmcro/trails/{cycle_id}/",
    "timestamp": "ISO-8601",
    "identity": {
      "owner": "Shawn",
      "company": "Tooensure",
      "stack": "MAF 1.7.0 + MCP 1.3.0 + PMCR-O 2.0.0"
    }
  }
}
```

---

## skill-delta.md

Written by the Orchestrator on ACCEPT if any EarnedConstraints should update skills.

```markdown
# Skill Delta — Cycle {cycle_id}

## Constraints Earned This Cycle

- EC-EARNED-2026-05-29-001: I will not pass unverified paths to ReadFile.
  Source: Loop 1 Maker failure, step 2.
  Recommended update: planner-agent/SKILL.md — add path verification step to protocol.

## Skills Recommended for Update

- skills/planner-agent/SKILL.md — add explicit path verification before plan emission
```

---

## Trail as Product — The Three Forms

**Form 1: Raw Trail**
The cycle output directory as produced. Machine-readable. Complete record.
Used for: debugging, auditing, feeding into the next cycle as `loopContext`.

**Form 2: Cognitive Asset**
The cycle-summary.json + skill-delta.md extracted and published.
Used for: team knowledge transfer, client deliverable, training data.

**Form 3: Compressed SKILL.md**
The patterns from the trail distilled into a new or updated SKILL.md.
Used for: distributing the solution as a reusable skill with identity injection slots.
The Trail is what was learned. The SKILL.md is the learning, packaged.

---

## Persistent EarnedConstraints

EarnedConstraints that appear in 3+ consecutive cycles promote to Colony Laws.
They are written to `.pmcro/constraints/{constraint_id}.json` and loaded
as part of the governance layer on all subsequent cycles.

```json
{
  "constraint_id": "EC-EARNED-2026-05-29-001",
  "rule": "I will not pass a path to ReadFile that was not verified to exist at plan time.",
  "cycles_triggered": 3,
  "promoted_to_law": true,
  "law_id": "EC-019",
  "promoted_timestamp": "2026-06-01T00:00:00Z"
}
```

→ Next: See `06-colony-laws.md`
