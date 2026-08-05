---
name: orchestrator
description: "USE FOR: sequencing a PMCR-O cycle end to end -- dispatching Planner, then Maker, then Checker, then Reflector, and deciding after each pass whether to loop or seal. This is the ONLY skill in the Colony that owns sequencing logic; every other role-skill (planner, maker, checker, reflector) is passive and stateless on its own. Use whenever a domain command says 'dispatch a PMCR-O cycle' or 'invoke pmcro-loop'. DO NOT USE FOR: doing planning, making, checking, or reflecting yourself -- delegate to the matching role-skill. DO NOT USE FOR: domain-specific judgment (that's the chief whose scope the cycle falls under, read via dependency-resolver before Planner starts)."
metadata:
  pmcro_provides: "orchestrator"
  pmcro_requires: "planner,maker,checker,reflector,dependency-resolver"
compatibility: "No external runtime dependency. Composes the four passive role-skills in this same skills/ catalog plus dependency-resolver. This is the single implementation referenced by every domain's SKILL.md as 'pmcro-loop' -- domains never reimplement this."
---

# Orchestrator

The Orchestrate role of the PMCR-O loop, and the one skill in this Colony
allowed to contain sequencing logic. Every domain-scoped cycle runs the
full Plan -> Make -> Check -> Reflect sequence before this skill decides
loop-vs-seal. No domain or command skips a role, regardless of how obvious
the fix seems.

This is the macro cascade point: the Colony's C-Suite chiefs (ceo, cto,
coo, cfo, cro, cmo, clo, chro, chief-of-staff, domain-specialist) are each
a domain scope this skill can dispatch a cycle against. Every dispatch is
its own micro cycle -- a full Plan->Make->Check->Reflect pass that writes
its own frame and jsonl files under `.pmcro/trails/<domain>/<uuid>/`, per
`references/trail-schema.md`. The macro/micro split is the same
progressive-disclosure principle the skill format itself uses: this skill
decides *which* chief's cycle runs and *whether it loops*, the chief's own
cycle logs *what actually happened* in its own trail.

## Why This Skill Exists Separately From Planner/Maker/Checker/Reflector

Composability requires passivity: a skill that contains its own sequencing
logic is coupled to one workflow and can't be invoked standalone elsewhere.
Planner, Maker, Checker, and Reflector are each independently useful and
independently testable -- Checker, for instance, can validate a Maker
output that came from somewhere other than this exact loop. If sequencing
were smeared across all five roles, none of them would compose. This skill
is where "what runs next" lives, full stop.

## Usage (System Contract)

A caller (a domain command, e.g. `/ceo:approve-initiative`, or
`/orchestrator:run-cycle` directly) invokes this skill with:

```
domain: <one of the C-Suite domains>
true_intent: <free-text description of the cycle's goal>
repo_path: <the target repo root>
```

This matches `commands/run-cycle.md`'s own parameter block exactly --
`run-cycle` is the dispatcher every domain command calls through, so its
output shape is this skill's input contract by construction. Keep the two
in sync if either ever changes.

## The Cycle

### 1. Resolve dependencies

Before dispatching Planner, invoke `dependency-resolver` with this cycle's
`domain` and `true_intent`. It returns which domain `SKILL.md` to read for
scope, and confirms `planner`/`maker`/`checker`/`reflector` are present and
compatible. Do not skip this even when the answer seems obvious -- a stale
or renamed domain path fails loudly here, not silently three roles later.
Write dependency-resolver's returned resolution to `00-deps.json` at the
trail root before creating `00-frame.json` -- this is what makes step 1
checkable rather than assumed; see `references/trail-schema.md`.

### 2. Create the trail

Generate a fresh UUID (`scripts/new_trail_id.py`, python3 `uuid.uuid4()`)
and create `.pmcro/trails/<domain>/<uuid>/` at `repo_path`. See
`references/trail-schema.md` for the exact frame/file layout before
writing anything.

### 3. Plan -> Make -> Check -> Reflect

Dispatch each role-skill in order, passing it the trail path and the
previous role's output file:

1. **planner** writes `00-frame.json` and `01-plan.jsonl`
2. **maker** reads the frame + plan, writes `01-make.jsonl`
3. **checker** reads the frame + make, writes `01-check.jsonl`
4. **reflector** reads the frame + check, writes `01-reflect.jsonl` --
   including crystallizing an EarnedConstraint if a pattern recurs 3+
   times (see `references/earned-constraints.md`)

Never invoke a role out of this order, and never let a later role's output
overwrite an earlier one's file in place -- each cycle number gets its own
`NN-*.jsonl` set, per the trail schema.

### 4. Decide: loop or seal

Read the cycle's `01-check.jsonl` disposition. If Checker flagged a
failure Reflector's constraint doesn't resolve on its own, increment the
cycle number and repeat step 3 -- Planner re-plans against the Reflector's
new constraint, not from scratch. If Checker passed, or Reflector's
constraint closes the gap, seal:

- Write `disposition.json` at the trail root with one of:
  `accept` | `needs-approval` | `reject` | `needs-revision`
- `accept` is only valid for cycles that touched nothing requiring HIL
  (see `references/hil-gating.md` for the TYPE1/TYPE2 boundary). Any
  cycle that wrote to `catalog/`, a `marketplace.json`, or another
  domain's `SKILL.md` seals `needs-approval` regardless of how clean the
  check passed -- that gate is not this skill's to waive.

### 5. Bound the loop -- EC-009

Never loop unboundedly. The Colony's standing constraint is
**EC-009: MaxLoops = 3** per domain-scoped cycle. If cycle 3 still hasn't
produced a Checker pass, seal `needs-revision` and stop -- do not attempt
a 4th pass silently. See `references/earned-constraints.md` for the full
EC registry.

## Multi-Turn Evolution Work

Some cycles (e.g. scaffolding a full new domain plugin) genuinely need
more sustained iteration than a single-session Plan/Make/Check/Reflect
pass comfortably holds. For that shape of work, see
`references/ralph-loop-mechanics.md` -- bounded persistent iteration via
Claude Code's native `/goal` command, still respecting EC-009's cap.

## Reference Files

- `references/trail-schema.md` -- exact frame/trail file formats and
  directory layout
- `references/earned-constraints.md` -- EC-XXX registry format, EC-009
  definition, how to add a new constraint
- `references/ralph-loop-mechanics.md` -- bounded multi-turn iteration for
  cycles too large for one pass
- `references/hil-gating.md` -- TYPE1/TYPE2 tool boundary, which
  disposition a cycle is allowed to self-seal with
