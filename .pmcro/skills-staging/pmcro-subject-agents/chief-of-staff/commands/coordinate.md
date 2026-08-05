---
description: "Coordinate a cross-domain initiative without owning the specialist work. Usage: /chief-of-staff:coordinate <initiative>"
---
# /chief-of-staff:coordinate

```
initiative: <first argument>
repo_path: <the target repo root>
```

Cross-domain coordination. Chief-of-Staff does not own the specialist work.

## Steps

1. Identify every domain that has an Owns stake in the initiative.
2. Dispatch `/orchestrator:run-cycle chief-of-staff "coordinate: <initiative>"`.
3. Produce a coordination plan that names owners, dependencies, and decision points that must escalate to CEO.
4. Never absorb work that belongs to another domain’s Owns section.

## Guardrails
- Coordination is not execution.
- If a single domain clearly owns the work, route there instead of opening a coordination cycle.
