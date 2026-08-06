# Pattern D — Macro-Scoped Domain Loop (opt-in)

**Status:** Adopted 2026-08-05, explicit sign-off from the architecture owner
(Shawn), in the same conversation that raised it. Confirmed, not a silent
addition — see `cascade-and-omode.md` Confirmation status.

## What it is

A C-Suite domain skill (ceo, cto, coo, cfo, cro, cmo, chro, clo,
chief-of-staff, domain-specialist) may run its own bound Plan-Make-Check-Reflect
loop when — and only when — it is invoked as the **macro / top-level entry
point** of a dispatch. Example: a person or another system calls `cto:cto`
directly, not as a mid-plan consult from a Planner already running a cycle.

## What it is NOT

Pattern B (ad-hoc consult, see `cascade-and-omode.md` §2) is unchanged. If a
C-Suite skill is consulted *mid-plan* by another agent's Planner, it still
answers bound to the calling frame — no loop, no independent trail, no seal.
Pattern D never overrides Pattern B. The trigger is strictly about *who called
it and why*, not which skill it is.

## Trigger condition (must be checkable, not vibes)

Pattern D fires only if ALL of the following hold:
1. The domain skill is the **first** frame in the call stack for this
   request — nothing above it is already mid-Plan.
2. The domain skill's own SKILL.md declares `pattern_d: opt-in` (see each
   C-Suite SKILL.md's "Macro-Loop (Pattern D)" section).
3. The request's O-Mode decision (made by whatever dispatched to this skill)
   selected "Full PMCR-O cycle" as the strategy, not pass-through/CoT/etc.

If any one is false, the skill answers directly — it does not default into
Pattern D just because it's a domain skill.

## Trail & disclosure requirements

- A Pattern D loop seals its **own** trail, under
  `.pmcro/trails/<domain>/<uuid>/`, exactly like existing per-domain trails
  already in this repo (e.g. `trails/cto/`, `trails/ceo/`).
- It must disclose, in its own first frame, that it is running as Pattern D
  and not Pattern B — one line is enough: `"Actor":"<Domain>","Mode":"PatternD"`.
- It does NOT get a parent-trail-id back to whatever dispatched it, unless
  that dispatcher was itself an Orchestrator cycle — Pattern D loops invoked
  directly (e.g. a person calling `cto:cto` straight from chat) have no
  parent to link to, and that's fine; the trail stands alone.
