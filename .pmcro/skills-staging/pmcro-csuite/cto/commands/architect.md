---
description: "Design or validate a PMCR-O loop, skill pack, or technical architecture change. Usage: /cto:architect <scope>"
---
# /cto:architect

```
scope: <first argument – e.g. "new skill pack for property-preservation" or "Pattern D hardening">
repo_path: <the target repo root>
```

Technical architecture and PMCR-O loop / skill-pack design.

## Steps

1. Confirm the scope is technical architecture, loop design, security posture, DevOps, or incident-related (CTO Owns).
2. Dispatch `/orchestrator:run-cycle cto "<scope>"`.
3. Require an explicit Owns / Does-Not-Own boundary check against other C-Suite domains before any design is accepted.
4. Output must include concrete, falsifiable validation criteria that Checker can evaluate.
5. Do not implement the design inside this command — design and validate only.

## Guardrails
- Never silently absorb work that belongs to another domain's Owns section.
- Prefer widening an existing domain over proposing a new one (see `/ceo:evolve-colony` guardrails).
