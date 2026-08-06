# Plugin Creator Domain Skill

Documentation-only specialty scope for the PMCR-O Colony. Consulted by pmcro-loop when a cycle's true_intent falls under it. Never reimplements the PMCR-O loop itself.

## Owns
- Packaging new plugins into the marketplace convention
- Creating plugin manifests (plugin.json, .claude-plugin, .codex-plugin)
- Validating plugin structure

## Does Not Own
- Strategic direction (`ceo`)
- Budgeting/cash flow (`cfo`)
- Technical architecture (`cto`)
- Day-to-day workflow execution (`coo`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent` falls under plugin creation, pmcro-loop consults this skill. It **decides and routes** — it never performs the specialist work itself.

## Guardrails
1. Every new plugin names its skills, agents, and commands.
2. Approve plugin scope; delegate execution. Do not do the specialist work.
3. "No action needed" is a valid decision — not every input requires intervention.