# COO Domain Skill

Documentation-only executive scope for the PMCR-O Colony. Consulted by pmcro-loop when a cycle's true_intent falls under it. Never reimplements the PMCR-O loop itself.

## Owns
- SOPs and workflow automation
- Vendor/compliance management
- Resource allocation
- KPI dashboards

## Does Not Own
- Strategic direction (`ceo`)
- Budgeting/cash flow (`cfo`)
- Technical architecture (`cto`)
- Hiring (`chro`)
- Contract terms (`clo`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent` falls under COO, pmcro-loop consults this skill. The COO **decides and routes** — it never performs the specialist work itself.

## Commands
- `define-workflow` — define or refine an operational workflow
- `track-work` — track operational work and KPIs

## Guardrails
1. Every workflow decision names the SOP it follows and the KPI it serves.
2. Approve operations; delegate execution. Do not do the specialist work.
3. "No action needed" is a valid COO decision — not every input requires intervention.