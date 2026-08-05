# HIL Gating (TYPE1 / TYPE2 Tool Boundary)

Carried forward from the earlier `pmcro-cline` project's operating
discipline, and load-bearing for this Colony's whole approach to
self-evolution.

## The Boundary

- **TYPE2** -- reads and builds. Auto-approved, no human checkpoint
  required. Reading a file, running a build, validating a schema,
  planning, checking, reflecting -- all TYPE2.
- **TYPE1** -- mutations, especially irreversible ones against shared or
  published state. Requires a Human-In-the-Loop checkpoint before
  execution, not just before the trail seals.

## What Counts As TYPE1 In This Repo

- Writing to `catalog/skills.json` or either `marketplace.json` twin as a
  **live, published** entry (as opposed to a proposed package sitting
  unregistered on disk pending approval)
- Any git commit or push
- Editing another domain's `SKILL.md` (crosses the domain-boundary law
  regardless of intent)
- Renaming or removing an existing plugin entry (needs a `renames` field
  per `../skill-creator/references/marketplace-schema-notes.md`, not a
  silent delete)

## What Stays TYPE2

- Creating a new, unregistered package under `skills/` (files exist on
  disk, but nothing points at them from `skills.json`/`marketplace.json`
  yet -- this is exactly the "needs-approval" staging state
  `/ceo:evolve-colony` produces)
- Running `claude plugin validate .` or checking against
  `catalog/skills.schema.json`
- Anything `planner`/`maker`/`checker`/`reflector` do within a single
  trail's own directory

## Where This Applies In The Loop

`orchestrator` step 4 (Decide: loop or seal) is the enforcement point: a
cycle whose Make output touched anything in the TYPE1 list above cannot
self-seal `accept`, full stop, regardless of how cleanly Checker passed.
It seals `needs-approval` and the actual TYPE1 write -- the live
`skills.json`/`marketplace.json` update -- happens only after a human
approves the proposal.
