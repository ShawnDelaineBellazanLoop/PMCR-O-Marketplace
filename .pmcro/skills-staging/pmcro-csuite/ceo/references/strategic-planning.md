# Strategic Planning Reference — CEO Domain

Frameworks and decision authority for the Colony's top-level direction.

## The CEO Decision Framework

Every strategic decision the CEO makes falls into one of these categories:

| Category | Definition | Action |
|---|---|---|
| Direction | Where the company/colony is going | Set, communicate, measure |
| Allocation | Who gets what resources | Decide, document, delegate |
| Approval | Go/no-go on major initiatives | Review context, decide, record rationale |
| Intervention | Something needs CEO override | Act, explain why, set guardrails for next time |
| No Action | This doesn't need CEO attention | Say so explicitly — silence reads as indecision |

"No action needed" is a real decision. State it when it applies.

## OKR Structure

Every OKR in this Colony follows:

```
Objective: [Qualitative direction — where we're going]
  Key Result 1: [Quantitative measure — how we know we got there]
  Key Result 2: ...
  Key Result 3: ...
```

Rules:
- 3-5 Objectives per quarter. More than 5 means you're not prioritizing.
- 2-5 Key Results per Objective. Each KR must be measurable.
- KRs are outcomes, not tasks. "Launch feature X" is a task. "Feature X drives 20% increase in Y metric" is a KR.
- Every KR has an owner domain. No orphan KRs.

## Priority Allocation

When multiple domains compete for attention, score each initiative:

```
Priority Score = (Impact × Urgency) + CrossDomainDependency

Impact:    1-5 (how much does this matter to company direction?)
Urgency:   1-5 (what happens if we wait a week?)
CrossDomainDependency: +2 if initiative unlocks another domain's work
```

Tiebreaker: the initiative that unblocks the most other work wins.

## Decision Record Format

Every CEO approval records:

```json
{
  "decision_id": "ceo-YYYY-MM-DD-NNN",
  "initiative": "what was proposed",
  "proposed_by": "domain/agent",
  "decision": "APPROVED | REJECTED | DEFERRED | NEEDS_MORE_INFO",
  "rationale": "why — this is the part that prevents re-litigation",
  "conditions": ["any guardrails attached to the approval"],
  "delegated_to": "domain that executes"
}
```
