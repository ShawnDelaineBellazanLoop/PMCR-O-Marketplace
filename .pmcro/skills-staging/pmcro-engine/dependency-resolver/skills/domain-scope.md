# Dependency Resolver Domain Skill

Resolves which domain scope and PMCR-O role-skills a cycle needs via `metadata.pmcro_provides` / `pmcro_requires`.

## Role

The Dependency Resolver walks existing `metadata.pmcro_provides` / `pmcro_requires` declarations across the catalog to confirm a capability is actually missing and not just uncalled. It prevents proposing a skill that duplicates one that exists.

## Key Design Rules

1. **Check before proposing** — before proposing anything new, walk existing `pmcro_provides` / `pmcro_requires` declarations to confirm a capability is actually missing.
2. **No duplicate skills** — don't propose a skill that duplicates one that exists.
3. **Resolve scope** — determines which domain scope and PMCR-O role-skills a cycle needs.

## Guardrails

1. Never propose a new domain when an existing one's `Owns` plausibly covers the gap.
2. Every proposed gap names the evidence for it — an actual missing capability observed in real work, not a hypothetical.
3. Read-only inventory and classification — never modifies files.