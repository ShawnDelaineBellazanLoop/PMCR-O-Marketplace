---
description: "Produce or update a marketing strategy artifact. Usage: /cmo:campaign <name>"
---
# /cmo:campaign

```
name: <first argument>
repo_path: <the target repo root>
```

Marketing strategy and campaign definition under CMO authority.

## Steps

1. Confirm the request is marketing strategy (CMO Owns).
2. Dispatch `/orchestrator:run-cycle cmo "campaign or strategy: <name>"`.
3. Output must include audience, channel, message, success metric, and owner.
4. Budget implications must be handed to CFO; technical delivery implications to CTO/COO.

## Guardrails
- Do not invent performance numbers.
- Keep claims falsifiable so Checker can evaluate them later.
