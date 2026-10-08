---
description: "Open or advance a hiring / people-ops process. Usage: /chro:people-ops <action>"
---
# /chro:people-ops

```
action: <first argument – e.g. "open req for backend engineer" or "review onboarding checklist">
repo_path: <the target repo root>
```

People-operations and hiring under CHRO authority.

## Steps

1. Confirm the request falls under CHRO Owns (hiring, people operations).
2. Dispatch `/orchestrator:run-cycle chro "<action>"`.
3. Output must name any dependencies on other domains (budget → CFO, legal terms → CLO, role definition → CTO/COO).
4. Do not write offer letters or employment contracts — that is CLO territory.

## Guardrails
- Stay inside people-ops scope.
- Escalate compensation or headcount decisions that affect runway to CFO + CEO.
