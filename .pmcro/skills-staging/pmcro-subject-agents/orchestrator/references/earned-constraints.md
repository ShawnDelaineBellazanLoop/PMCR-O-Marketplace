# Earned Constraints (EC Registry)

An EarnedConstraint is a lesson crystallized by `reflector` after the same
finding recurs 3+ times across cycles. It becomes a standing rule every
future cycle in this Colony checks against -- not a suggestion, a
constraint `checker` enforces going forward.

## Format

Each entry: an ID, a short imperative statement, and what it resolves.

```
EC-009: MaxLoops = 3 per domain-scoped cycle.
  Resolves: unbounded Plan->Make->Check->Reflect re-looping.
  Raised: seeded at Colony founding (not reflector-crystallized -- see
  "Seed Constraints" below).
```

## Seed Constraints (present from Colony founding, not crystallized)

These didn't emerge from 3+ recurrences -- they're load-bearing enough to
seed directly rather than wait for a pattern to repeat:

- **EC-009**: MaxLoops = 3 per domain-scoped cycle. `orchestrator`
  enforces this at step 5 of every cycle. Raising this ceiling
  deliberately (not by looping past it) requires a `/ceo:evolve-colony`
  cycle of its own, reviewed by `cto`, not a unilateral Orchestrator
  decision mid-cycle.
- **EC-VERIFY-FIRST-001**: Verify actual disk/repo state before acting on
  or claiming it. `checker` treats a claim about state that wasn't
  verified against the actual target as a finding, not a pass.
- **EC-011**: The two marketplace surfaces (`.claude-plugin/marketplace.json`
  and `.agents/plugins/marketplace.json`, if both exist in a given repo)
  must be written in the same pass and must never drift on plugin roster,
  `version`, `displayName`, or the top-level `description`.
