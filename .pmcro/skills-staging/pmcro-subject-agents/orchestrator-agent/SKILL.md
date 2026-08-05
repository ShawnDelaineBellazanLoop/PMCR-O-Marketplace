---
name: orchestrator
description: Run a PMCR-O (Plan-Make-Check-Reflect-Orchestrate) cognitive cycle for tasks that genuinely need plan/make/check/reflect discipline — multi-step work, work where correctness matters and should be checked against source claims rather than assumed, or work that should produce an auditable trail. Use this whenever the user asks to "run a cycle," "orchestrate," references PMCR-O, O-Mode, Pattern A/B, an "earned constraint," or the constraint ledger, or when a task's complexity or stakes call for planning, execution, per-claim verification, and reflection rather than a single direct answer. Do not force trivial requests through this — see O-Mode below, which explicitly includes "no frame needed" as a valid choice.
---

# Orchestrator

Runs the PMCR-O cognitive cycle: Orchestrator decides *how* a request should be
reasoned about (O-Mode) and *what scope* it falls under, then dispatches to four
agents in sequence — Planner, Maker, Checker, Reflector — each defined in its own
file under `agents/`.

This skill is built entirely from a live design conversation and its three source
documents, carried into `references/`. Read `references/cascade-and-omode.md`
before running a cycle for the first time in a session — it is the architecture,
not just background. `references/constraint-ledger.md` is a living document:
Reflector appends to it; nothing else should.

## Core principle

A PMCR-O frame describes how *one actor* reasoned through *one step*. It is not a
requirement that every agent call triggers a new cycle. Only the Orchestrator
orchestrates — an agent called mid-plan is not acting as an Orchestrator, does not
spin up its own cycle, and does not seal its own trail.
## Step 1 — O-Mode: pick the reasoning strategy

Before doing anything else, decide what shape this request actually needs. This is
a menu — "run the full cycle" is one option, not the default:

| Strategy | When it applies |
|---|---|
| Trivial pass-through | No frame needed — just answer |
| Full PMCR-O cycle | Task needs plan/make/check/reflect discipline |
| Iterative refinement | Checker deliberately kicks a step back until valid — a chosen strategy, not a retry storm |
| Chain-of-thought | Step-by-step reasoning; tail can re-seed as the next intent |
| Tree/graph-of-thought | Task needs exploring multiple branching paths |
| ReAct | Interleaved reason-then-act for tool-heavy exploratory work |
| Elicitation | Right move is surfacing a decision point back to the human, not planning silently |

If the answer is "no frame needed," give the direct answer and stop — do not
manufacture a cycle to justify this skill having triggered.

## Step 2 — Domain scope (Pattern A)

If a full cycle is warranted, decide the scope up front, before Planner starts —
if the request clearly belongs to one coherent domain, name it once here so
Planner, Maker, Checker, and Reflector all run bound to it. If nothing forces a
domain, proceed untagged rather than inventing one — see the open question on
this in `references/cascade-and-omode.md` §5.2.
## Step 3 — Dispatch the cycle

**Note on Pattern D (added 2026-08-05):** if the incoming request names a
single C-Suite domain directly as its top-level entry point — not something
Planner is consulting mid-plan — check whether that domain's SKILL.md
declares `pattern_d: opt-in`. If so, the domain skill may run its own bound
loop and seal its own trail instead of this Orchestrator dispatching
Planner/Maker/Checker/Reflector itself. See
`references/pattern-d-macro-loop.md` for the exact trigger conditions —
this is a real branch point in Step 3, not a footnote, so check it before
assuming the four-agent dispatch below always applies.

In order, invoke:

1. `agents/planner.md` — produces the plan. May consult another agent mid-plan
   (Pattern B: bound sub-call, same frame, no new trail — see the agent file for
   the exact rules).
2. `agents/maker.md` — executes the plan, produces the artifact. Surfaces gaps in
   its own reasoning as they're caught, not smoothed over afterward.
3. `agents/checker.md` — checks the artifact **per claim**, not as one pass/fail
   verdict. Labels each claim VERIFIED / SYNTHESIZED / UNVERIFIED / CONTRADICTED,
   and ends with an explicit ACCEPT or NEEDS-REVISION disposition.
4. `agents/reflector.md` — reads what Checker actually caught. If something
   genuine and recurring surfaced, crystallizes it into a new entry in
   `references/constraint-ledger.md`, following that file's format law exactly.
   Recommends whether O-Mode should switch strategy for the next cycle.

Each frame speaks in first person ("I am the Planner, cycle N...") and states
which parts of its output are established vs. constructed. This isn't decoration —
it's what lets Checker do a per-claim pass instead of a rubber stamp, and it's why
Reflector has to account for its own evaluation sitting one recursive layer above
frames that are themselves self-referential (EC-002).

## Step 3.5 — Human-in-the-loop between frames (conversational runs only)

When this cycle is being run conversationally (not inside the .NET runtime's own
`DispatchType1Async` HIL gate, which already exists for TYPE1 tool calls), pause
for explicit human confirmation **after each frame**, not just at the end:

1. Present Planner's frame. Wait for confirmation before Maker runs.
2. Present Maker's frame. Wait for confirmation before Checker runs.
3. Present Checker's per-claim verdict + disposition. Wait for confirmation
   before Reflector runs.
4. Present Reflector's frame (including any new EC or NextSeedIntent) as the
   final seal.

This is the conversational analogue of the same principle the real code already
enforces for TYPE1 actions: a human confirms the *real* state before the next
phase treats it as ground truth, rather than the loop chaining four frames
unattended and presenting only the final result. Do not collapse this to "run
all four then summarize" — that defeats the reason for asking.

## Step 4 — Seal or loop

- If Checker's disposition is **ACCEPT**: seal the cycle. Present the artifact,
  along with any per-claim SYNTHESIZED items that still need the user's
  confirmation before they're treated as settled.
- If **NEEDS-REVISION**: do not silently "fix" it into an ACCEPT. Either loop back
  to Planner with Reflector's re-seeded intent (the error itself, not the old
  intent with a footnote), or — if the same EC has now hit its recurrence
  threshold — switch O-Mode strategy per Reflector's recommendation rather than
  retrying the same approach.

## Constraints that apply across every cycle

- **EC-001 — Layer Boundary Constraint.** A cycle at the cognitive-architecture
  layer never cites implementation-layer artifacts (specific paths, specific tool
  names, product mechanics) as evidence — name the category, not the instance.
- **EC-002 — Reflector Recursion Constraint.** Once every frame in a cycle
  self-references, Reflector's evaluation is structurally one layer higher than
  usual — expected, not an error to flatten.

Full provenance for both is in `references/constraint-ledger.md`. Read it before a
cycle, and only append to it via Reflector, in the exact five-part format it
specifies.

## What this explicitly does not do

- Does not give a called agent its own Orchestrator instance, independent trail,
  or authority to invoke O-Mode / domain-scoping on its own (Pattern B stays
  bound). **Exception, 2026-08-05:** a C-Suite domain skill invoked as the
  top-level entry point (not mid-plan) and declaring `pattern_d: opt-in` may
  seal its own trail — this is Pattern D, a deliberate, narrowly-scoped
  addition, not a loosening of the Pattern B rule above.
- Does not track parent-trail-id or depth ceilings for Pattern B consults — there
  is no second cycle to bound.
- Does not treat the cascade diagram or any specific example mapping in
  `references/cascade-and-omode.md` as settled — both are explicitly flagged
  unconfirmed at the bottom of that file. Surface them to the user for
  confirmation the first time they'd matter to a real decision, rather than
  citing them as precedent.
