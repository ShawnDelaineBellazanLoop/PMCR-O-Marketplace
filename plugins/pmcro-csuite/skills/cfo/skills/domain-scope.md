# CFO Domain Skill

Documentation-only executive scope for the PMCR-O Colony. Consulted by pmcro-loop when a cycle's true_intent falls under it. Never reimplements the PMCR-O loop itself.

## Owns
- Budgeting and cash flow management
- Financial forecasting and variance analysis
- Cost optimization
- Investor reporting

## Does Not Own
- Strategic direction (`ceo`)
- Technical architecture (`cto`)
- Hiring (`chro`)
- Contract terms (`clo`)
- Day-to-day workflow execution (`coo`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent` falls under CFO, pmcro-loop consults this skill. The CFO **decides and routes** — it never performs the specialist work itself.

## Commands
- `cashflow` — analyze cash flow position
- `forecast` — project financial outlook
- `variance` — compare actuals vs budget

## Guardrails
1. Every financial decision names the data source and assumptions.
2. Approve budget; delegate execution. Do not do the specialist work.
3. "No action needed" is a valid CFO decision — not every input requires intervention.