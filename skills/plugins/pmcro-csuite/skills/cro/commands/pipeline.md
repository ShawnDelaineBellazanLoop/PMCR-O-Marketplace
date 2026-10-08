---
description: "Pipeline or sales-methodology action. Usage: /cro:pipeline <action>"
---
# /cro:pipeline

```
action: <first argument>
repo_path: <the target repo root>
```

Sales methodology and pipeline movement under CRO authority.

## Steps

1. Confirm the action is sales / pipeline work (CRO Owns).
2. Run `/orchestrator:run-cycle cro "pipeline: <action>"`.
3. Output must name stage, next concrete action, and any blocker that belongs to another domain.
4. Revenue forecasts that affect cash position must be coordinated with CFO.

## Guardrails
- Never invent deal status or close dates.
- Pipeline numbers must be traceable to a source system or sealed trail.
