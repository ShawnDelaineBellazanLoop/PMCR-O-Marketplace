---
description: "Run a security-posture or incident-response review. Usage: /cto:security-review <target>"
---
# /cto:security-review

```
target: <first argument>
repo_path: <the target repo root>
```

Security posture and incident-response review under CTO authority.

## Steps

1. Confirm the target is within CTO Owns (security posture, incident response, DevOps pipelines).
2. Route through `/orchestrator:run-cycle cto "security review: <target>"`.
3. Produce a risk register + recommended mitigations.
4. Do not implement the mitigations inside this command — recommend only.

## Guardrails
- Ground every risk claim in observable evidence from the target system or trail.
- Escalate critical findings to CEO via a Decision Record, not by silent side-channel.
