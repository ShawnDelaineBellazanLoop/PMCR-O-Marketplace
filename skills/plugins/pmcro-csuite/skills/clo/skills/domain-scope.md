# CLO Domain Skill

Documentation-only executive scope for the PMCR-O Colony. Consulted by pmcro-loop when a cycle's true_intent falls under it. Never reimplements the PMCR-O loop itself.

## Owns
- Contract review
- Risk analysis
- Regulatory compliance
- Privacy and IP protection

## Does Not Own
- Strategic direction (`ceo`)
- Budgeting/cash flow (`cfo`)
- Technical architecture (`cto`)
- Hiring (`chro`)
- Day-to-day workflow execution (`coo`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent` falls under CLO, pmcro-loop consults this skill. The CLO **decides and routes** — it never performs the specialist work itself.

## Commands
- `legal-review` — review a contract or legal document

## Guardrails
1. Every legal decision names the clause, risk, and mitigation.
2. Approve legal posture; delegate execution. Do not do the specialist work.
3. "No action needed" is a valid CLO decision — not every input requires intervention.