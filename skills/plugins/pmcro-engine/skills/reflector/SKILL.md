---
name: reflector
description: "USE FOR: the Reflect role of a PMCR-O cycle -- reading a cycle's check output and, when a failure pattern recurs, crystallizing it into an EarnedConstraint (EC-XXX) for future cycles. Invoked by orchestrator after checker has run, on both pass and fail cycles. DO NOT USE FOR: fixing the immediate failure (that's a re-plan, driven by orchestrator looping back to planner), deciding whether to loop or seal (orchestrator)."
metadata:
  pmcro_provides: "reflector"
  pmcro_requires: ""
compatibility: "Read access to this trail's prior cycles and to the EC registry (../orchestrator/references/earned-constraints.md) to check for recurrence."
---

# Reflector

The Reflect role of the PMCR-O loop. Distills lessons, does not re-execute
anything. Runs on every cycle, not just failed ones -- a clean pass can
still surface a pattern worth crystallizing before it causes a failure
somewhere else.

## Invocation Contract

Called by `orchestrator` with:

```
trail_path: <.pmcro/trails/<domain>/<uuid>/>
cycle_number: <NN>
check: <path to NN-check.jsonl>
prior_cycles: <this trail's earlier NN-check.jsonl files, if cycle_number > 01>
```

## What To Do

1. Read this cycle's `check.jsonl` disposition and findings.
2. Check `../orchestrator/references/earned-constraints.md` (the EC
   registry) and this trail's `prior_cycles` for whether the same finding
   (or a close variant) has occurred **3 or more times**. Three is the
   threshold; do not crystallize a one-off into a standing constraint, and
   do not wait past three occurrences to raise one.
3. If the threshold is met, write a new EC entry: a short, imperative,
   checkable statement, plus which check ID(s) it resolves. Append it to
   the registry -- do not silently duplicate an existing EC under a new
   number.
4. Write `<cycle_number>-reflect.jsonl`: what was reflected on, whether a
   new EC was raised (and which one), and if this cycle should loop, what
   the re-plan should incorporate that the prior plan didn't.

## What Not To Do

- Do not crystallize a constraint from a single occurrence -- that's
  premature generalization, and it pollutes the registry for every future
  cycle that reads it.
- Do not modify Checker's findings to fit a constraint you'd like to raise
  -- the finding is upstream fact, the constraint is downstream synthesis.
- Do not attempt the fix yourself. This skill closes with a recommendation
  for the next Plan, not a patch.

