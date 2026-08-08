---
name: trail-indexer
description: >
  Reads sealed PMCR-O trails from .pmcro/trails/{subjectAgent}/{trailId}/ and
  produces a structured index summarizing each trail's seed intent, disposition,
  cycle count, and key artifacts. Operates against the REAL LoopFrame.cs schema,
  not the generic Tier-0 template. Type-2 read-only; never writes to a trail.
license: Proprietary
metadata:
  author: tooensure
  version: "1.1.0"
  tier: SKILL
  maf-type: AgentFileSkill
  compatible-with: LoopFrame.cs as of 2026-07-23
requires: pmcro-framework
---

# Trail Indexer

You index sealed PMCR-O cognitive trails into a queryable summary form.

## Frame vocabulary — REAL schema (LoopFrame.cs / ITrailWriter.cs)

Do NOT use the generic Tier-0 template frame names. This project's actual on-disk
layout and field names are:

```
00-frame.json          { trail_id (string), seed_intent (string), started_utc (string) }
{NN}-plan.jsonl        one JSON per line: PlannerFrame { trail_id, seed_intent, project,
                          steps[].action, steps[].action_type (TYPE1|TYPE2), raw_plan,
                          cycle_number, success_criteria? }
{NN}-make.jsonl         one JSON per line: MakerFrame { trail_id, seed_intent, plan (PlannerFrame),
                          step_results[].step_index, step_results[].action, step_results[].ok,
                          step_results[].error?, step_results[].ground_truth.method,
                          step_results[].ground_truth.verified, step_results[].ground_truth.evidence,
                          all_steps_ok }
{NN}-check.jsonl        one JSON per line: CheckerFrame { trail_id, seed_intent, maker_output (MakerFrame),
                          check_items[].step_index, check_items[].passed,
                          check_items[].failure_evidence?, check_items[].criterion?, all_passed,
                          raw_verdict }
{NN}-reflect.jsonl      one JSON per line: ReflectorFrame { trail_id, seed_intent, disposition
                          (Accept|Retry|Halt), final_output, retry_context?, halt_reason?,
                          earned_constraints[{id, rule, triggered_by}], cycle_number,
                          raw_reflection, next_seed_intent? }
disposition.json        { trail_id, disposition (Accept|Retry|Halt), sealed_utc, earned_constraints[] }
```

Critical mapping corrections (verified 2026-07-23 against LoopFrame.cs):

| Template assumed | Real field/layout | Notes |
|---|---|---|
| SeedIntentFrame{subject,goal} | 00-frame.json.seed_intent | single field; no split |
| DispositionFrame{accept,reject,superseded} | disposition.json.disposition ∈ {Accept,Retry,Halt} | Retry never appears sealed |
| ObservationFrame.evidence | MakerFrame.step_results[].ground_truth.evidence where verified=true | No ObservationFrame type exists |
| CheckerFrame.checkItems[].evidence | CheckItem.failure_evidence (only when passed=false) | Passing items read from same cycle's MakerFrame.ground_truth |

## What you produce

For each sealed trail, emit one index record:

```json
{
  "trail_id": "...",
  "subject_agent": "...",
  "sealed_utc": "...",
  "disposition": "Accept|Retry|Halt",
  "cycle_count": N,
  "seed_intent_summary": "one sentence",
  "earned_constraints": [{ "id": "...", "rule": "..." }],
  "artifact_paths": [ "{NN}-plan.jsonl", "{NN}-make.jsonl", "{NN}-check.jsonl", "{NN}-reflect.jsonl" ]
}
```

## Rules

- Type-2 only. Read trails; never write, modify, or delete any trail content.
- A trail is sealed only if disposition.json exists with Disposition ∈ {Accept, Halt}.
- Retry trails are mid-loop; index them but mark disposition=Retry.
- earned_constraints may be [] (FileTrailWriter seals with an empty array at seal time).
- If a field is absent, write null — do not fabricate.
- Cite the exact file and JSON field you read for every derived value.