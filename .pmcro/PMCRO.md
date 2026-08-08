# PMCRO.md — Runtime Root Manifest

Every agent loads this file before executing a cycle (EC-001).
This is the entry point into the PMCR-O runtime for this repo.

## Load Order

1. `.pmcro/PMCRO.md` (this file)
2. `.pmcro/identity.json` — injects `{{company}}`, `{{owner}}`, `{{project}}` tokens
3. `.pmcro/config.json` — runtime limits (MaxLoops, etc.)
4. `.pmcro/laws/colony-laws.md` — governance corpus (EC-001..EC-###)
5. `.pmcro/constraints/earned-constraints.json` — accumulated EarnedConstraints

## Canonical Root

`W:/PMCR_O/PMCR-O-Marketplace/.pmcro` — this repo is canonical as of
2026-08-05 (see `laws/POINTER.md` and `identity.json` for the
consolidation history).
