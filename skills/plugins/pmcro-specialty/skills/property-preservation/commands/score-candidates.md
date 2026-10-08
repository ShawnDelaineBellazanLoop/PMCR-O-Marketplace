---
description: "Score and qualify vacant/ghost properties from Beacon data. Usage: /property-preservation:score-candidates <batch-or-criteria>"
---
# /property-preservation:score-candidates

```
criteria: <first argument>
repo_path: <the target repo root>
```

Qualify preservation candidates from Ramsey County Beacon records.

## Steps

1. Confirm the request is county-record research / ghost-property scoring (property-preservation Owns).
2. Require evidence from at least two signals:
   - tax delinquency (nonzero delinquent balance)
   - owner mailing address differs from property address
   - ownership held by estate or trust
3. Dispatch `/orchestrator:run-cycle property-preservation "score candidates: <criteria>"`.
4. Every extracted property must cite its Beacon source record. If the scraper returns nothing, say so.

## Guardrails
- Never fabricate county-record data.
- A single signal alone is not a qualifying flag.
- This skill does not own chat/session distillation — route that to domain-specialist.
