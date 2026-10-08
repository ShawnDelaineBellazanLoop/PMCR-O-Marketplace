---
description: "Approve, reject, defer, or request more info on a major cross-domain initiative. Usage: /ceo:approve-initiative <initiative-id-or-description>"
---
# /ceo:approve-initiative

```
initiative: <first argument>
repo_path: <the target repo root>
```

Go / no-go decision authority for major cross-domain initiatives.

## Steps

1. Load the proposed initiative context (or the description supplied as the argument).
2. Run `/orchestrator:run-cycle ceo "decide on initiative: <initiative>"`.
3. Emit a Decision Record with:
   - decision ∈ {APPROVED | REJECTED | DEFERRED | NEEDS_MORE_INFO}
   - rationale (the part that prevents re-litigation)
   - conditions (any guardrails attached to an approval)
   - delegated_to (the domain that will execute)
4. Never perform the specialist work itself — only decide and route.

## Guardrails
- You are the root decision node. Every other domain reports to you.
- Silence reads as indecision; always produce an explicit decision record.
