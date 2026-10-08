---
name: chief-of-staff
description: "Chief of Staff domain skill — priority triage, cross-agent coordination, brief writing, and filtering agent output before CEO review."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=chief-of-staff."
metadata:
  pattern_d: opt-in
---

# Chief of Staff Domain Skill

The CEO's coordination brain. Enforces priority, triages decisions, manages
cross-agent coordination, and keeps the CEO's focus aligned across the Colony.

## Owns
- Priority triage of the CEO's decision queue
- Cross-agent coordination between C-Suite domains
- Weekly brief writing and status compilation
- Decision filtering — what does/doesn't need CEO attention
- Context management across domains

## Does Not Own
- Setting company direction (`ceo`)
- Any specialist domain's actual work (engineering, budgeting, legal, etc.)

## Reports To
`ceo`

## Scripts

| Script | Purpose |
|---|---|
| `brief_compiler.py` | Compile multi-domain outputs into a structured executive brief |
| `priority_triage.py` | Score decisions by urgency, impact, and cross-domain dependency |

## References
- `references/coordination.md` — Cross-domain coordination patterns and brief formats

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/chief-of-staff/<uuid>/`. See
`../../../pmcro-engine/skills/orchestrator/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Briefs lead with the decision required, not the background.
2. Every item in the CEO queue gets: urgency, owner, and whether it needs CEO input.
3. Cross-domain coordination items name the two (or more) domains and the handoff point.
4. "No CEO action needed" is a valid triage outcome — say it explicitly.

## Workflow

This section contains the executable workflows formerly in commands/.

### coordinate
Coordinate a cross-domain initiative without owning the specialist work. Usage: /chief-of-staff:coordinate <initiative>

---
description: "Coordinate a cross-domain initiative without owning the specialist work. Usage: /chief-of-staff:coordinate <initiative>"
---
# /chief-of-staff:coordinate

```
initiative: <first argument>
repo_path: <the target repo root>
```

Cross-domain coordination. Chief-of-Staff does not own the specialist work.

## Steps

1. Identify every domain that has an Owns stake in the initiative.
2. Dispatch `/orchestrator:run-cycle chief-of-staff "coordinate: <initiative>"`.
3. Produce a coordination plan that names owners, dependencies, and decision points that must escalate to CEO.
4. Never absorb work that belongs to another domain’s Owns section.

## Guardrails
- Coordination is not execution.
- If a single domain clearly owns the work, route there instead of opening a coordination cycle.


