---
name: planner
description: "USE FOR: the Plan role of a PMCR-O cycle -- reading a true_intent plus the relevant domain's Owns/Does-Not-Own scope, and producing the frame and plan for that cycle. Invoked by orchestrator, never invoked standalone by a domain command directly. DO NOT USE FOR: executing the plan (maker), validating output (checker), deciding whether to loop (orchestrator), or sequencing anything -- this skill is deliberately passive and stateless."
metadata:
  pmcro_provides: "planner"
  pmcro_requires: ""
compatibility: "No external runtime dependency. Reads only what orchestrator hands it plus the target domain's SKILL.md."
---

# Planner

The Plan role of the PMCR-O loop. This skill is intentionally passive: it
has no opinion about what happens before or after it runs, which is what
lets it be reused outside this exact five-role sequence if ever needed.

## Invocation Contract

Called by `orchestrator` with:

```
domain: <C-Suite domain this cycle is scoped to>
true_intent: <free-text goal for this cycle>
trail_path: <.pmcro/trails/<domain>/<uuid>/ at repo_path>
cycle_number: <NN, starts at 01>
prior_reflect: <path to previous cycle's NN-reflect.jsonl, if this is a re-plan>
```

## What To Do

1. Read the target domain's `skills/<domain>/SKILL.md` (path resolved by
   `dependency-resolver` before this skill was invoked). Confirm
   `true_intent` actually falls under that domain's Owns section. If it
   doesn't, or spans another domain's Owns section too, say so in the frame
   rather than silently planning past the boundary -- domain-boundary law
   is not this skill's to waive.
2. If `prior_reflect` is present, read it first. A re-plan must plan
   against the Reflector's crystallized constraint from last cycle, not
   restate the original plan unchanged.
3. Write `<cycle_number>-frame.json` (only on cycle 01 -- later cycles in
   the same trail reuse the trail's one frame and only add new
   `NN-plan.jsonl` files) per the format in
   `../orchestrator/references/trail-schema.md`.
4. Write `<cycle_number>-plan.jsonl`: an ordered list of concrete steps
   Maker should take, each step naming what "done" looks like so Checker
   has something falsifiable to check against. A step with no verifiable
   success condition is not a complete plan step.

## What Not To Do

- Do not execute anything -- no file writes outside the trail directory,
  no tool calls that mutate state. Planning is read-and-write-the-plan
  only.
- Do not decide whether to loop or seal -- that's `orchestrator`, after
  `checker` and `reflector` have run.
- Do not silently absorb work that belongs to a different domain's Owns
  section just because it's adjacent to this cycle's true_intent.

