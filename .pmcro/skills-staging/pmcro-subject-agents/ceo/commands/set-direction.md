---
description: "Set or update the Colony’s strategic direction and quarterly OKRs. Usage: /ceo:set-direction <objective-text>"
---
# /ceo:set-direction

```
objective: <first argument – qualitative direction>
repo_path: <the target repo root>
```

Set or revise the Colony’s top-level direction. This is a CEO-only action.

## Steps

1. Confirm the request falls under CEO Owns (strategic planning, OKR management, direction-setting).
2. Invoke `/orchestrator:run-cycle ceo "set or revise direction: <objective>"`.
3. Require the cycle to produce a Decision Record matching the format in `references/strategic-planning.md`:
   - decision_id, initiative, proposed_by, decision, rationale, conditions, delegated_to.
4. Do not execute any specialist domain work — only set direction and name the owning domain for follow-on work.
5. “No action needed” is a valid outcome; state it explicitly when it applies.

## Guardrails
- Approve strategy; never perform the specialist work.
- Every OKR must have a measurable Key Result and an owner domain.
- Do not invent metrics or invent ownership.
