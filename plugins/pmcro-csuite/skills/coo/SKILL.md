---
name: coo
description: "COO domain skill — SOP creation, workflow automation, vendor management, compliance enforcement, resource allocation, and KPI dashboards."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=coo."
pattern_d: opt-in
---

# COO Domain Skill

Runs the machine. Creates SOPs, automates workflows, manages vendors, enforces
compliance, allocates resources, and maintains KPI dashboards.

## Owns
- SOP creation and maintenance
- Workflow automation and process design
- Vendor management and relationship tracking
- Compliance enforcement
- Resource allocation
- KPI dashboards and operational metrics

## Does Not Own
- Financial reporting (`cfo`)
- Legal compliance review (`clo`)
- Technical architecture (`cto`)
- Hiring process design (`chro`)

## Reports To
`ceo`

## Manages
`domain-specialist` — industry-specific execution reports into COO.

## Scripts

| Script | Purpose |
|---|---|
| `kpi_dashboard.py` | Compute KPIs from metric data, flag threshold violations |
| `sop_validator.py` | Validate SOP completeness against required sections and format |

## References
- `references/operations.md` — Operational excellence frameworks

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/coo/<uuid>/`. See
`../../../pmcro-legacy/skills/orchestrator-agent/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Process is the deliverable. An answer without a repeatable process is incomplete.
2. Every SOP states: who, what, when, and the handoff point.
3. Vendor issues get categorized: performance, cost, compliance, relationship.
4. KPI dashboards surface red/yellow/green — state the threshold, not just the color.

## Workflow

This section contains the executable workflows formerly in commands/.

### define-workflow
Create or update an operational SOP / workflow. Usage: /coo:define-workflow <name>

---
description: "Create or update an operational SOP / workflow. Usage: /coo:define-workflow <name>"
---
# /coo:define-workflow

```
name: <first argument>
repo_path: <the target repo root>
```

Define or revise a day-to-day operational workflow under COO authority.

## Steps

1. Confirm the workflow is day-to-day execution (COO Owns), not strategy (CEO) or technical architecture (CTO).
2. Dispatch `/orchestrator:run-cycle coo "define workflow: <name>"`.
3. Output must be concrete enough for a downstream domain (e.g. property-preservation) to execute without further interpretation.
4. Record any cross-domain hand-offs explicitly.

## Guardrails
- Do not design company strategy or technical architecture inside this command.
- Prefer existing SOPs; only create a new one when a real operational gap is demonstrated.


### track-work
Track and close operational work orders or commitments. Usage: /coo:track-work <work-id-or-description>

---
description: "Track and close operational work orders or commitments. Usage: /coo:track-work <work-id-or-description>"
---
# /coo:track-work

```
work: <first argument>
repo_path: <the target repo root>
```

Operational work-order tracking and close-out.

## Steps

1. Confirm the work item is operational execution (COO Owns).
2. Run `/orchestrator:run-cycle coo "track or close work: <work>"`.
3. Produce status, blockers, and next concrete action.
4. When the work belongs to a specialist domain (e.g. property-preservation), name that domain and hand off rather than absorbing the specialist steps.

## Guardrails
- Never fabricate status.
- Close-out requires evidence that the success criteria of the original work order were met.


