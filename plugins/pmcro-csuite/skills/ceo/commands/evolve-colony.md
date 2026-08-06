---
description: Find capability gaps in the Colony (missing skills, missing domains, or mis-scoped ones) and route each to the right fix -- new command, new sub-agent, or new domain -- before anything gets built.
---

# /ceo:evolve-colony

Run the CEO agent-router pass: pull everything the Colony currently knows,
find what's missing or mis-scoped, and decide what kind of thing should
fill each gap. This command decides *if* and *what kind* -- it does not
build anything itself.

## Steps

1. **Inventory.** List every skill under `skills/` and read each
   `SKILL.md` frontmatter (`name`, `description`) plus `Owns` / `Does Not
   Own` / `Reports To`. This is "everything we know."

2. **Check for gaps, using `dependency-resolver` first.** Before proposing
   anything new, walk existing `metadata.pmcro_provides` /
   `pmcro_requires` declarations across the catalog to confirm a
   capability is actually missing and not just uncalled. Don't propose a
   skill that duplicates one that exists.

3. **Classify each real gap** as one of:
   - A **command** — a new slash-invokable workflow inside an existing
     domain (like this file). Use when the capability fits squarely
     inside one domain's existing `Owns`.
   - A **sub-agent** — a bound, narrower specialist inside an existing
     domain that needs its own focused context. Use when a domain's
     scope is right but a task within it needs isolated execution.
   - A **new domain** — a whole new `Owns`/`Does Not Own`/`Reports To`
     skill. Use only when no existing domain's `Owns` covers the gap,
     even loosely. This is the highest bar -- check twice before
     proposing a new domain.

4. **Check content/description drift.** If a skill's on-disk content
   doesn't match its frontmatter `description` or its expected role
   (the kind of mismatch that produced `property-preservation` as a
   sibling to `domain-specialist`), flag it as its own gap rather than
   silently reconciling it.

5. **Report, don't build.** Output a numbered list: each gap, its
   classification (command / sub-agent / new domain), which existing
   domain it belongs under (or why none fits), and one line of
   reasoning. Building happens in a separate cycle, via `skill-creator`,
   after this list is reviewed.

## Guardrails
1. Never propose a new domain when an existing one's `Owns` plausibly
   covers the gap, even if the fit is imperfect -- widen the existing
   domain first, split later only if it actually gets overloaded.
2. Every proposed gap names the evidence for it -- an actual missing
   capability observed in real work, not a hypothetical.
3. This command does not modify any file. It is read-only inventory and
   classification. `skill-creator` (invoked separately, after review) is
   what writes anything.
