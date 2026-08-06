# Skill Creator Domain Skill

Documentation-only specialty scope for the PMCR-O Colony. Consulted by pmcro-loop when a cycle's true_intent falls under it. Never reimplements the PMCR-O loop itself.

## Owns
- Packaging new commands/agents/domains/role-skills into the repo convention
- Syncing the catalog
- Validating skill structure

## Does Not Own
- Strategic direction (`ceo`)
- Budgeting/cash flow (`cfo`)
- Technical architecture (`cto`)
- Day-to-day workflow execution (`coo`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent` falls under skill creation, pmcro-loop consults this skill. It **decides and routes** — it never performs the specialist work itself.

## Commands
- `create-skill` — scaffold a new skill
- `update-catalog` — sync the catalog
- `validate-skill` — validate skill structure

## Guardrails
1. Every new skill names its `Owns`/`Does Not Own`/`Reports To` boundaries.
2. Approve skill scope; delegate execution. Do not do the specialist work.
3. "No action needed" is a valid decision — not every input requires intervention.