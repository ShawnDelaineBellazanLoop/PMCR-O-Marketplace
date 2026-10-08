# Operations Reference — COO Domain

Operational excellence frameworks for process design and KPI management.

## SOP Structure

Every SOP in this domain follows:

```markdown
# SOP: [Name]

## Owner
[Domain/role responsible]

## Trigger
[What event starts this process]

## Inputs
[What information/materials are needed to start]

## Steps
1. [Action] → [Output]
2. [Action] → [Output]

## Handoff
[Who receives the output and what they do with it]

## SLA
[Time commitment: response time, completion time]

## Exceptions
[Known edge cases and how to handle them]
```

## KPI Categories

Operational KPIs fall into four buckets:

| Category | Example KPIs | Healthy Range |
|---|---|---|
| Throughput | Tasks completed/period, cycle time | Trending up or stable |
| Quality | Error rate, rework rate, compliance % | <5% error, >95% compliance |
| Responsiveness | Time-to-acknowledge, time-to-resolve | Within SLA for 95th percentile |
| Efficiency | Cost per unit, automation rate | Trending down for cost, up for automation |

## KPI Dashboard Rules

1. Every KPI has an explicit threshold that triggers yellow and red.
2. Dashboards show trend (last 4 periods) not just current value.
3. A red KPI without a recommended action is incomplete.
4. Green across the board means thresholds are too loose.

## Vendor Management

Vendor issues fall into one of:

| Issue Type | Owner | Escalation Path |
|---|---|---|
| Performance | COO → renegotiate or replace | If SLA breach, notify CEO |
| Cost | CFO → budget review | If >15% over, CEO approval for continuation |
| Compliance | CLO → legal review | If regulatory exposure, immediate CLO + CEO |
| Relationship | COO → account management | Escalate to CEO if strategic partner at risk |
