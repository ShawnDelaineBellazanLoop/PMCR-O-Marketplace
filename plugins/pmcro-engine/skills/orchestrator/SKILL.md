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



## Workflow

This section contains the executable workflows formerly in commands/.


### replay-trail
Replay a sealed trail's history for review or audit -- reconstructs the cycle-by-cycle narrative from its frame and jsonl files without re-executing anything. Usage: /orchestrator:replay-trail <trail-uuid>

---
description: "Replay a sealed trail's history for review or audit -- reconstructs the cycle-by-cycle narrative from its frame and jsonl files without re-executing anything. Usage: /orchestrator:replay-trail <trail-uuid>"
---
```
trail_id: <first argument>
repo_path: <the target repo root>
```

Read-only. Read `00-frame.json`, then each cycle's
`plan.jsonl -> make.jsonl -> check.jsonl -> reflect.jsonl` in order, then
`disposition.json`. Present as a narrative: what was asked, what was tried
each cycle, what failed and why, what constraint (if any) got
crystallized, and how it ultimately sealed.

Sealed trails are immutable (`../references/trail-schema.md`) -- this
command never writes to the trail directory, including to "fix" something
noticed during replay. A defect found in a sealed trail gets logged as an
open hypothesis in a **new** trail, per the same immutability rule that
governs sealed trails generally.


### run-cycle
Dispatch a full PMCR-O cycle scoped to a given domain. This is the one entry point every domain command uses instead of reimplementing the loop. Usage: /orchestrator:run-cycle <domain> <true_intent>

---
description: "Dispatch a full PMCR-O cycle scoped to a given domain. This is the one entry point every domain command uses instead of reimplementing the loop. Usage: /orchestrator:run-cycle <domain> <true_intent>"
---
Invoke the `orchestrator` skill directly (not via a domain command -- this
is the underlying primitive domain commands themselves call).

```
domain: <first argument -- one of the C-Suite domains>
true_intent: <remaining arguments>
repo_path: <the target repo root>
```

Runs the full sequence in `../SKILL.md`: resolve dependencies, create the
trail, Plan -> Make -> Check -> Reflect (looping up to EC-009's cap of 3),
then seal with the appropriate disposition.

Prefer calling this indirectly through a domain command
(`/ceo:set-direction`, `/cto:...`, etc.) in normal use -- domain commands
supply the right `true_intent` framing for their scope. Call this directly
only when testing the loop itself, or when a genuinely cross-cutting cycle
doesn't yet have a domain command to hang off of.


### seal-trail
Manually seal a trail that's stuck (e.g. mid-cycle interruption, MCP instability during writes) after verifying its actual on-disk state. Usage: /orchestrator:seal-trail <trail-uuid>

---
description: "Manually seal a trail that's stuck (e.g. mid-cycle interruption, MCP instability during writes) after verifying its actual on-disk state. Usage: /orchestrator:seal-trail <trail-uuid>"
---
```
trail_id: <first argument>
repo_path: <the target repo root>
```

Before sealing anything: **EC-VERIFY-FIRST-001 applies.** Read every file
actually present in `.pmcro/trails/<domain>/<trail_id>/` -- do not trust
what a prior turn claimed was written. List the directory, read what's
actually there, then decide.

If the last complete cycle has a `check.jsonl` disposition but no
`disposition.json` at the trail root, write one now per
`../references/trail-schema.md`, using the same TYPE1/TYPE2 rule in
`../references/hil-gating.md` to pick `accept` vs `needs-approval`.

If cycle files are incomplete or contradictory (e.g. a `make.jsonl` with
no matching `plan.jsonl`), do not paper over the gap -- seal
`needs-revision` and note exactly what's missing, so a fresh cycle can
pick it up cleanly rather than building on an uncertain foundation.


