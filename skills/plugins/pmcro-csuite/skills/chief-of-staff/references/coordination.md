# Coordination Reference — Chief of Staff

Cross-domain coordination patterns and executive brief formats.

## Executive Brief Format

Every weekly brief follows:

```markdown
# CEO Brief — [Week Ending Date]

## Decisions Required This Week
[Items that need CEO input — name the decision, not the background]
| Item | Domain | Urgency | Recommendation |

## Status by Domain
### CEO
[Strategic initiatives, OKR progress]

### CTO
[Architecture decisions, incidents, deployments]

### CFO
[Cash position, budget status, forecast updates]

### COO
[Operations, compliance, vendor updates]

### CRO
[Pipeline health, deals closed, deals at risk]

### CMO
[Campaign performance, content output, brand metrics]

### CLO
[Contract reviews, regulatory updates, risk items]

### CHRO
[Hiring pipeline, onboarding, culture updates]

## Cross-Domain Coordination
[Items spanning 2+ domains — state the handoff point]

## Risks & Blockers
[What could derail this week's priorities]
```

## Priority Triage Matrix

When filtering what reaches the CEO:

```
                    URGENT              NOT URGENT
IMPORTANT       → CEO decides NOW     → CEO decides, schedule
NOT IMPORTANT   → Chief of Staff      → Delegate or drop
                  decides/delegates
```

## Coordination Handoff Pattern

Every cross-domain coordination names:

```json
{
  "initiative": "what's being coordinated",
  "domains": ["domain-a", "domain-b"],
  "handoff_point": "domain-a delivers X to domain-b by date",
  "blocker": "what's preventing progress",
  "owner": "domain that owns the next action"
}
```

## Brief Compilation Rules

1. Lead with decisions, not status.
2. Every status item is one sentence. If it needs more, it's a decision item.
3. Items that appear in 3+ consecutive briefs without resolution → escalate to CEO decision.
4. Positive news gets one line. Problems get context + recommendation.