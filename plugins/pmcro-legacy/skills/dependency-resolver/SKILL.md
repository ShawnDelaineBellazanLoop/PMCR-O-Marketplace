---
name: dependency-resolver
description: "USE FOR: resolving which domain SKILL.md and which PMCR-O role-skills a cycle needs, by walking metadata.pmcro_provides/pmcro_requires across this repo's skills/ catalog before a cycle dispatches. Invoked by orchestrator at the start of every cycle, and by skill-creator before packaging a new skill. DO NOT USE FOR: making the routing judgment call between ambiguous domains -- that's ceo's agent-router. This skill resolves declared dependencies; it doesn't guess intent."
metadata:
  pmcro_provides: "dependency-resolver"
  pmcro_requires: ""
compatibility: "Read access to every skill's SKILL.md frontmatter under this repo's skills/ directory."
---

# Dependency Resolver

Walks this repo's `provides`/`requires` graph so nothing gets dispatched
against a skill that isn't actually present, compatible, or up to date.

## Why This Exists As Its Own Skill

Both `orchestrator` (resolving role-skills before a cycle) and
`skill-creator` (resolving what a new skill would need before packaging it)
need the exact same graph-walk. Putting it in either of those would
duplicate the logic in the other, or couple one to the other unnecessarily.
One resolver, two callers -- same non-duplication principle as the loop
itself having exactly one implementation.

## Invocation Contract

```
requesting_skill: <name of the skill/domain asking>
needs: <comma-separated list of pmcro_provides values required, or "auto" to read requesting_skill's own pmcro_requires>
```

## What To Do

1. Read each candidate skill's `SKILL.md` frontmatter under `skills/` for
   its `metadata.pmcro_provides`/`pmcro_requires` fields -- this repo's
   marketplace.json doesn't carry a first-class dependency field (see
   `../skill-creator/references/marketplace-schema-notes.md`), so the
   declared contract lives in each skill's own frontmatter, which is the
   authoritative source.
2. For each `needs` entry, find exactly one package whose
   `metadata.pmcro_provides` matches. Zero matches or more than one match
   is a resolution failure -- report it rather than picking one silently.
3. Recursively resolve each matched package's own `pmcro_requires`, same
   rule, until the full dependency set bottoms out at leaf skills
   (`pmcro_requires: ""`).
4. Return the resolved set as an ordered list (dependencies before
   dependents), so the caller can confirm nothing stale is being
   dispatched against.

## What Not To Do

- Do not resolve a dependency by name-guessing when `pmcro_provides` values
  don't match exactly -- a near-miss is a packaging defect to surface, not
  something to paper over.
- Do not cache a prior resolution across cycles -- skill frontmatter can
  change between cycles (that's the whole point of `skill-creator`
  existing), so resolve fresh each time.
