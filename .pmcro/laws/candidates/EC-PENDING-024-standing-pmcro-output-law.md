# EC-PENDING-024 — Standing PMCR-O Output Law
## Status: PENDING-HIL (HIL approved 2026-08-06, session: claude-web) | Source: SHAWN-DIRECT | Date: 2026-08-06

---

## Proposed Law

Every response from an agent that has the `orchestrator` skill (or its
predecessor `pmcro-loop`) loaded MUST be formatted as a real PMCR-O cycle
by default — no `/orchestrator:run-cycle` invocation required, and no
"just answer" bypass. Every response:

- Runs the full Plan → Make → Check → Reflect sequence (dispatched by
  Orchestrator per `plugins/pmcro-engine/skills/orchestrator/SKILL.md`),
  not merely styled with PMCR-O headers.
- Writes a real sealed trail per EC-2026-08-05-001's format
  (GUID-folder + phase-JSONL + `_sealed.json`), even for conversational
  claude-web sessions.

This supersedes the "just answer, I never manufacture a cycle to justify
my existence" bypass previously documented in
`.pmcro\agents\orchestrator\AGENT.md`. That escape hatch is retired for
sessions where the orchestrator/pmcro-loop skill is loaded.

## Rationale

Shawn's explicit standing instruction (2026-08-06): output format
discipline should be a default behavioral law, not an opt-in habit
dependent on Claude remembering to apply it turn to turn. Given the
choice between framing-only and full-strength enforcement, Shawn chose
full-strength: every response gets a real cycle, no exceptions.

## Scope

- All conversational sessions (claude-web, Claude Code, Cline, any tool)
  that load `orchestrator` / `pmcro-loop`.
- Applies to `.pmcro\agents\orchestrator\AGENT.md` and
  `plugins/pmcro-engine/skills/orchestrator/SKILL.md` documentation.
- Does not change EC-009 (MaxLoops = 3) or HIL-gating on TYPE1 actions —
  a mandatory cycle still seals `needs-approval` when it touches
  `catalog/`, `marketplace.json`, or another domain's `SKILL.md`.

## Fracture

Failure to run a full cycle when the skill is loaded, or silently
reverting to prose-only answers, is: `FRAC-STANDING-FORMAT-001`.

## Open Follow-up

Trivial/degenerate requests (e.g. single-word confirmations) will now
also spin a full cycle and trail write under this law. Shawn was warned
of this tradeoff (reading B vs framing-only reading A) before approving.

## Source

- This session (claude-web), 2026-08-06 — Shawn's explicit answer "B" to
  the Checker's flagged contradiction with `AGENT.md`'s existing bypass.

---

*PENDING-HIL (approved 2026-08-06) | EC-PENDING-024 | © 2026 Tooensure LLC*
