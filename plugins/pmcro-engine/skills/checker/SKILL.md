---
name: checker
description: "USE FOR: the Check role of a PMCR-O cycle -- validating make output against the plan's stated success criteria, this repo's schemas, and applicable Colony law. Invoked by orchestrator after maker has run. DO NOT USE FOR: fixing what failed (reflector crystallizes the lesson, a re-plan fixes the work), deciding whether to loop (orchestrator). This skill only judges pass/fail against explicit criteria -- it does not do the work of the thing it's checking."
metadata:
  pmcro_provides: "checker"
  pmcro_requires: ""
compatibility: "No external runtime dependency beyond read access to whatever the plan's success criteria reference (schemas, prior frames, target files)."
---

# Checker

The Check role of the PMCR-O loop. Judges, does not fix. Every finding here
must trace to something explicit -- a plan step's stated success condition,
a schema, or a named Colony law -- never a vibe.

## Invocation Contract

Called by `orchestrator` with:

```
trail_path: <.pmcro/trails/<domain>/<uuid>/>
cycle_number: <NN>
frame: <path to 01-frame.json>
plan: <path to NN-plan.jsonl>
make: <path to NN-make.jsonl>
```

## What To Check, In Order

1. **Plan-conformance** -- did Make's output satisfy each plan step's
   stated success condition? A step with no stated condition is itself a
   Checker finding against Planner, not something to wave through.
2. **Schema-conformance** -- if the cycle touched a catalog index or
   schema file, validate against it. If it touched a `marketplace.json`,
   note that Claude Code's own `claude plugin validate` CLI checks a
   different surface than any repo-local schema and both should pass;
   flag if only one was actually run.
3. **Dependency-resolution-recorded** -- confirm `00-deps.json` exists at
   the trail root and its `resolved` list actually matches the role-skills
   this cycle dispatched. Missing file or a mismatch between `resolved`
   and what actually ran is a finding against `orchestrator`, not a pass
   with a note.
4. **Law-conformance** -- re-read the Colony Law block (four numbered laws:
   One Bounded Action Per Cycle, TYPE1/TYPE2 Discipline, Ground Truth
   Honesty, Action Scope) as embedded in the relevant subject agent's own
   SKILL.md (e.g. `../terminal-agent/SKILL.md`,
   `../filesystem-agent/SKILL.md`,
   `../playwright-agent/SKILL.md` under the
   pmcro-engine plugin), cross-checked against the constraint ledger at
   `../orchestrator/references/constraint-ledger.md`
   -- against what Make actually did, not what the plan said it would do.
   There is no central `.agents/rules/` folder; Colony Law lives per
   subject-agent SKILL.md and the ledger is the cross-cutting record.
5. **HIL-boundary conformance** -- confirm any TYPE1 action Maker took had
   the gate `../orchestrator/references/hil-gating.md` requires, not
   just a plan step authorizing it.
6. **Catalog-Consistency** -- if the cycle touched
   `.claude-plugin/marketplace.json`, confirm its plugin roster,
   `version`, and `displayName` per entry are internally consistent, and
   note explicitly whether a second marketplace twin exists in this repo
   before checking twin-parity -- do not assume one exists.

Write `<cycle_number>-check.jsonl` with one entry per finding: which check,
pass/fail, and for failures, the specific evidence (not "seems off").

## Disposition Signal

End `check.jsonl` with a single summary entry the way `orchestrator` reads
it: `pass` (all checks passed), or `fail` (at least one check failed, with
the failing check IDs listed). This is the signal Reflector and Orchestrator
act on -- do not bury it in prose.

## What Not To Do

- Do not rewrite Make's output to make a check pass.
- Do not soften a fail into a "pass with notes" -- Reflector needs the real
  gap to crystallize a useful constraint.

