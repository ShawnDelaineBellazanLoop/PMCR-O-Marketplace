---
description: "Create or update an operational SOP / workflow. Usage: /coo:define-workflow <name>"
---
# /coo:define-workflow

```
name: <first argument>
repo_path: <the target repo root>
```

Define or revise a day-to-day operational workflow under COO authority.

## Steps

1. Confirm the workflow is day-to-day execution (COO Owns), not strategy (CEO) or technical architecture (CTO).
2. Dispatch `/orchestrator:run-cycle coo "define workflow: <name>"`.
3. Output must be concrete enough for a downstream domain (e.g. property-preservation) to execute without further interpretation.
4. Record any cross-domain hand-offs explicitly.

## Guardrails
- Do not design company strategy or technical architecture inside this command.
- Prefer existing SOPs; only create a new one when a real operational gap is demonstrated.
