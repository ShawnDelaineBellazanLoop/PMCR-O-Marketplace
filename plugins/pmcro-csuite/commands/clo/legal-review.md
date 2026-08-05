---
description: "Review or draft contract / legal terms. Usage: /clo:legal-review <matter>"
---
# /clo:legal-review

```
matter: <first argument>
repo_path: <the target repo root>
```

Contract and legal-terms review under CLO authority.

## Steps

1. Confirm the matter is contract terms or legal review (CLO Owns).
2. Run `/orchestrator:run-cycle clo "legal review: <matter>"`.
3. Produce red-lines, open questions, and recommended next action.
4. Do not make business or technical design decisions — only legal assessment.

## Guardrails
- Never invent statutory or case-law citations.
- Flag any matter that requires external counsel rather than attempting to resolve it inside the Colony.
