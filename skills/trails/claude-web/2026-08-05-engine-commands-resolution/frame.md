# Frame — pmcro-engine commands/ resolution

**True intent:** Resolve whether `pmcro-engine` needs a root
`commands/` directory like `pmcro-csuite` and `pmcro-specialty` do.

## Plan
Compare structure of all three pmcro-* plugins; read the actual
planner/maker/checker/reflector/orchestrator SKILL.md descriptions to
see how each is meant to be invoked.

## Make (evidence gathered)
- `pmcro-csuite/commands/` and `pmcro-specialty/commands/` each hold
  one `.md` file per user-facing slash command (e.g.
  `ceo/approve-initiative.md`, `coo/track-work.md`) — these are
  named entry points a person or top-level agent invokes directly.
- `pmcro-engine/skills/{planner,maker,checker,reflector,orchestrator}`
  have no root `commands/` and none of their SKILL.md files reference
  a command surface. Each explicitly states it is invoked by
  `orchestrator`, never called standalone by a domain command:
  planner ("never invoked standalone by a domain command directly"),
  maker/checker/reflector (same pattern), orchestrator (invoked by a
  domain command saying "dispatch a PMCR-O cycle").

## Check
This matches the two-layer architecture already confirmed in memory:
MAF-native runtime skills (engine = the loop's internal machinery)
vs. FRAME-CONTRACT prose skills for reasoning (csuite/specialty =
user/domain-facing). Root `commands/` is the marketplace convention
for the *latter* layer only.

## Reflect / Disposition
**pmcro-engine is command-less by design — not a gap.** Its five
skills are the cycle's internal primitives, always dispatched
programmatically by `orchestrator`'s own sequencing logic, never
typed by a user as a slash command. Adding a root `commands/` to
engine would misrepresent it as user-invokable and would violate the
"orchestrator is the only skill that owns sequencing" rule already
in the orchestrator SKILL.md.
