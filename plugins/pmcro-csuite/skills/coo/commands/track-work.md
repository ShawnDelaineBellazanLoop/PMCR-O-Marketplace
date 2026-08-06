---
description: "Track and close operational work orders or commitments. Usage: /coo:track-work <work-id-or-description>"
---
# /coo:track-work

```
work: <first argument>
repo_path: <the target repo root>
```

Operational work-order tracking and close-out.

## Steps

1. Confirm the work item is operational execution (COO Owns).
2. Run `/orchestrator:run-cycle coo "track or close work: <work>"`.
3. Produce status, blockers, and next concrete action.
4. When the work belongs to a specialist domain (e.g. property-preservation), name that domain and hand off rather than absorbing the specialist steps.

## Guardrails
- Never fabricate status.
- Close-out requires evidence that the success criteria of the original work order were met.
