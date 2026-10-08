# Property Preservation Domain Skill

Documentation-only specialty scope for the PMCR-O Colony. Consulted by pmcro-loop when a cycle's true_intent falls under it. Never reimplements the PMCR-O loop itself.

## Owns
- Property inspection and condition assessment
- Candidate scoring for preservation work
- Inspection report generation

## Does Not Own
- Strategic direction (`ceo`)
- Budgeting/cash flow (`cfo`)
- Technical architecture (`cto`)
- Day-to-day workflow execution (`coo`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent` falls under property preservation, pmcro-loop consults this skill. It **decides and routes** — it never performs the specialist work itself.

## Commands
- `inspection-report` — generate a property inspection report
- `score-candidates` — score preservation candidates

## Guardrails
1. Every inspection decision names the property, condition, and evidence.
2. Approve preservation scope; delegate execution. Do not do the specialist work.
3. "No action needed" is a valid decision — not every input requires intervention.