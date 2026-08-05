# Cascade & O-Mode — PMCR-O Colony Architecture

**Status:** Draft architecture, derived from live design conversation, 2026-07-20.
**Checker disposition on this document as a whole: NEEDS-REVISION, not ACCEPT.**
Two specific items below are flagged UNCONFIRMED and must not be treated as settled
law until explicitly confirmed. See "Confirmation status" at the bottom before
extending this file.

---

## 0. The problem this resolves

Every agent-to-agent call raises one question: does the call spin up a whole new
orchestrated cycle, or not? Get it wrong either direction:

- Treat every call as unstructured delegation → no accountability, nothing auditable.
- Treat every call as a full new Orchestrator cycle → runaway recursive cascade, no
  depth ceiling, unreadable trails.

Neither extreme is right. There are three distinct patterns. Conflating them is what
causes the confusion.

## 1. Core principle: frames are cognitive units, not system loops

A PMCR-O frame (Plan → Make → Check → Reflect) describes how one actor reasoned
through one step. It is not a license, requirement, or side-effect of "an agent got
called." An agent can be invoked and do useful work without spinning up its own
Plan/Make/Check/Reflect loop — it can just answer, bound to the context it was
called with, and hand control back.

**Corollary:** Orchestrator is not a role other agents inherit by being called. Only
the Orchestrator orchestrates. An agent called mid-plan is not acting as an
Orchestrator, is not sealing its own trail, and is not free to route to other agents
on its own initiative — it is bound inside the calling agent's context.
## 2. The four patterns

*(Patterns A-C were the original three. Pattern D was added 2026-08-05 — see
§2.5 and the Confirmation status at the bottom.)*

### Pattern A — Macro-scoped cycle (domain-scoped from the start)

The Orchestrator decides the entire cycle's domain scope up front, before Planner
starts reasoning. Then Planner → Maker → Checker → Reflector all run bound within
that scope from the first frame onward.

- One trail. One domain. No cascade.
- Trigger: a domain-scope decision resolves the incoming intent to a domain
  *before* Planner starts.

### Pattern B — Ad-hoc consult (bound sub-call, same frame)

Mid-plan, an agent (typically Planner) needs a specialized voice and calls another
agent directly — the same way it would call any subject tool. The called agent:

- Does **not** spin up its own Orchestrator instance.
- Does **not** seal its own independent trail.
- Answers bound to the calling agent's context, scoped to the one question asked.
- Hands the answer back into the *same* Plan frame that called it. The frame gets
  richer content; it does not fork.

This is structurally identical to calling any tool. Which agent you called only
changes what kind of context it injects (domain judgment vs. a raw resource lookup)
— not the control flow.

**No parent-trail-id needed. No depth ceiling needed.** There is no nesting to
bound, because there is no second cycle — it's one more sub-step inside the same
cycle's existing frame.
### Pattern C — O-Mode (reasoning-strategy selection)

Before (or instead of) forcing every request through a full five-role cycle, the
Orchestrator first decides what cognitive shape the request actually needs. This is
a menu, not a fixed pipeline:

| Strategy | When it applies |
|---|---|
| Trivial pass-through | Simple input — no frame at all, just answer |
| Full PMCR-O cycle | Task genuinely needs plan/make/check/reflect discipline |
| Iterative refinement | Checker kicks a step back until valid, looping at the check step specifically, as a *deliberate* strategy — not an accidental retry storm |
| Chain-of-thought | Step-by-step reasoning; tail can be re-seeded as the next intent |
| Tree-of-thought / graph-of-thought | Task shape calls for exploring multiple branching paths, not one linear plan |
| ReAct | Interleaved reason-then-act, useful for tool-heavy exploratory tasks |
| Elicitation / multiple-choice | Right move is to surface a decision point back to the human (or another agent) rather than plan silently |

O-Mode is not a sixth role bolted onto Plan-Make-Check-Reflect-Orchestrate. It is
the Orchestrator's own dynamic-reasoning-selection layer, deciding which strategy
(including "none needed") a given request runs under. "Run the full five-role
frame" is one option on the menu, not the default every input is forced through.
### Pattern D — Macro-scoped domain loop (opt-in, per C-Suite skill)

**Added 2026-08-05, explicit sign-off from the architecture owner.** A C-Suite
domain skill (ceo, cto, coo, cfo, cro, cmo, chro, clo, chief-of-staff,
domain-specialist) may run its own bound Plan-Make-Check-Reflect loop and seal
its own trail when it is invoked as the **macro / top-level entry point** —
not when it's a Pattern B mid-plan consult. Full trigger conditions, trail
format, and disclosure rules live in `references/pattern-d-macro-loop.md`
rather than duplicated here.

This is the "fourth pattern" §4 previously named and explicitly deferred
("not what this skill builds"). It is now built, but scoped tightly: it never
overrides Pattern B, and a skill only gets it by declaring `pattern_d: opt-in`
in its own SKILL.md.

## 3. How the four patterns relate

```
Orchestrator
  │
  ├─ O-Mode: decide cognitive strategy for this intent
  │     (pass-through | full cycle | iterative | CoT | ToT/GoT | ReAct | elicit)
  │
  └─ domain-scope decision: decide domain for this cycle
        (Untagged | domain-A | domain-B | ... )
              │
              ▼
        Pattern A: cycle runs bound to that domain's scope
              │
              ▼
        Planner reasons within scope
              │
              ├─ Pattern B: Planner consults another agent mid-plan
              │     (bound sub-call, same frame, no new trail)
              │
              ▼
        Maker → Checker → Reflector → Seal
```

O-Mode and domain-scoping are **both** Orchestrator-level decisions, made before or
at the top of a cycle. Pattern B (consult) happens *inside* a cycle, at the
Planner/Maker level, and never re-triggers O-Mode or domain-scoping — it's a
lower-level, bound call.

## 4. What this explicitly rules out

- A called agent does **not** get to independently invoke O-Mode or domain-scoping.
  Those are Orchestrator-only decisions.
- A called agent does **not** seal its own trail as a side effect of being
  consulted (Pattern B). Only the owning cycle's trail gets sealed.
- Recursive lineage tracking and depth ceilings are **not needed for Pattern B**,
  because there is no second cycle spawned.
- **Update, 2026-08-05:** the "fourth pattern where a called agent spins up its
  own full independent cycle" is no longer purely hypothetical — see Pattern D
  (§2.5). It is scoped narrowly (macro/top-level entry only, opt-in per skill,
  never overriding Pattern B) precisely so it doesn't reintroduce the runaway
  recursion problem §0 warns about. Pattern B's rules above are otherwise
  unchanged.
## 5. Open questions (unresolved — do not silently answer these while running the loop)

1. Micro-frames around subject-agent tool calls -- RESOLVED, cycle 4,
   2026-08-05. Not a 4th pattern in *this* sense (that's Pattern D, added
   later the same day, and it's a different question — see §2.5). A
   malformed/truncated subject-agent response is handled as
   O-Mode's existing "iterative refinement" strategy applied at tool-call
   granularity (one call retried until valid) rather than forcing the whole
   macro cycle through a full Retry. No new pattern; same menu, smaller unit.
2. **Untagged cycles and domain-scoping.** Is an untagged cycle a deliberate bypass
   of domain-scoping, or a gap where domain-scoping should run and decide "no
   domain match, proceed generic" but currently doesn't fire?
3. **Disposition propagation for Pattern B.** If a consulted agent says "I can't
   answer this / out of scope," does that force the calling Planner to Retry its
   own frame, or can the Planner absorb that and re-plan without triggering a
   cycle-wide Retry?

## Confirmation status

Per the Checker's pass on the source document, everything above §5 is either
VERIFIED (stated directly in the design conversation) or a labeled SYNTHESIZED
connection. Two items are the largest inferential leaps and are **not yet
confirmed**:

- **§3's cascade diagram** (O-Mode and domain-scoping as parallel sibling
  decisions, Pattern B never re-triggering either) — architecturally consistent,
  but constructed by the Checker/Maker, not asserted outright in the source
  conversation.
- **§2 Pattern A's mapping to any specific existing trail** — the pattern itself is
  verified; a mapping to a *specific* prior trail as an example of it was not.

Until explicitly confirmed by the person who owns this architecture, treat both as
"proposed, working assumption" rather than settled Colony Law — do not cite them as
precedent inside a running cycle the way EC-001 forbids citing implementation
artifacts as evidence.

**Pattern D (§2.5), by contrast, IS explicitly confirmed** — Shawn approved it
directly, in the same conversation that proposed it, 2026-08-05, choosing
"skill-level design first, code later" as the build order. It is Colony Law,
not a working assumption, as of this edit. This is the difference between the
two UNCONFIRMED items above (Checker-constructed inferences awaiting owner
review) and Pattern D (owner-originated, owner-approved on the spot).
