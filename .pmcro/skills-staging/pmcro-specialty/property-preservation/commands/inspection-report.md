---
description: "Build a field inspection report from photos + notes. Usage: /property-preservation:inspection-report <property-id>"
---
# /property-preservation:inspection-report

```
property_id: <first argument>
repo_path: <the target repo root>
```

Assemble a structured field inspection report.

## Steps

1. Confirm the request is inspection reporting (property-preservation Owns).
2. Separate three distinct sections in the output:
   - observed condition
   - contractor recommendation
   - compliance status
3. Run `/orchestrator:run-cycle property-preservation "inspection report for <property_id>"`.
4. Do not blend the three sections into a single narrative note.

## Guardrails
- Photos and notes are evidence; do not invent observations.
- Compliance status must reference the applicable county standard, not a personal judgment.
