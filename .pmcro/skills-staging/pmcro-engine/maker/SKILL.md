---
name: maker
description: "USE FOR: the Make role of a PMCR-O cycle -- executing the current cycle's plan steps and writing the make output. Invoked by orchestrator after planner has produced a plan for this cycle. DO NOT USE FOR: deciding what to do (planner), validating what was done (checker), deciding whether to loop (orchestrator). This skill is passive and stateless -- it does exactly what the plan says, nothing it infers on its own."
metadata:
  pmcro_provides: "maker"
  pmcro_requires: ""
compatibility: "No external runtime dependency beyond whatever tools the plan step calls for (filesystem, bash, MCP, etc. -- scoped by the plan itself, not by this skill)."
---

# Maker

The Make role of the PMCR-O loop. Executes what `planner` planned. Does not
re-derive intent from `true_intent` directly -- if the plan is ambiguous or
incomplete, that's a planning defect to flag back through `checker`, not
something this skill should paper over by improvising.

## Invocation Contract

Called by `orchestrator` with:

```
trail_path: <.pmcro/trails/<domain>/<uuid>/>
cycle_number: <NN>
frame: <path to 01-frame.json>
plan: <path to NN-plan.jsonl>
```

## What To Do

1. Read the frame and this cycle's plan in full before acting on any single
   step -- later steps sometimes depend on context set by earlier ones.
2. Execute each plan step in order. For each step, record in
   `<cycle_number>-make.jsonl`:
   - which step this satisfies
   - what was actually done (file written, command run, tool called)
   - the concrete output/result, not a paraphrase of intent
3. Respect the TYPE1/TYPE2 tool boundary in
   `../orchestrator/references/hil-gating.md` -- a TYPE1 (mutative,
   irreversible) action the plan calls for still requires whatever HIL gate
   that boundary defines. A plan asking for a TYPE1 action doesn't itself
   constitute the approval.
4. If a step turns out to be unexecutable as planned (missing file, tool
   error, ambiguous target), stop and record that in `make.jsonl` rather
   than guessing a substitute action. Checker and Reflector need the real
   failure, not a silently-adjusted one.

## What Not To Do

- Do not decide the plan was wrong and re-plan yourself -- that's a
  Checker-flagged, Reflector-crystallized, re-Planned loop, not a Maker
  improvisation.
- Do not skip a plan step because it looks redundant -- if it's genuinely
  redundant, that's feedback for Checker to catch, not Maker's call.

