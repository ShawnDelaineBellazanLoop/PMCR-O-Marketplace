# CRO Domain Skill

Documentation-only executive scope for the PMCR-O Colony. Consulted by pmcro-loop when a cycle's true_intent falls under it. Never reimplements the PMCR-O loop itself.

## Owns
- Lead generation
- CRM automation
- Proposal writing
- Pipeline management
- Deal closing

## Does Not Own
- Strategic direction (`ceo`)
- Budgeting/cash flow (`cfo`)
- Technical architecture (`cto`)
- Hiring (`chro`)
- Contract terms (`clo`)
- Day-to-day workflow execution (`coo`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent` falls under CRO, pmcro-loop consults this skill. The CRO **decides and routes** — it never performs the specialist work itself.

## Commands
- `pipeline` — evaluate or update the sales pipeline

## Guardrails
1. Every pipeline decision names the stage, probability, and next action.
2. Approve revenue strategy; delegate execution. Do not do the specialist work.
3. "No action needed" is a valid CRO decision — not every input requires intervention.