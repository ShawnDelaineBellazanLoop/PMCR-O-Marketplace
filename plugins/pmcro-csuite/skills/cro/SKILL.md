---
name: cro
description: "CRO domain skill — lead generation, CRM automation, outreach, pipeline management, proposal writing, and deal closing."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=cro."
metadata:
  pattern_d: opt-in
---

# CRO Domain Skill

Drives revenue. Automates lead generation, manages CRM pipelines, executes
outreach campaigns, writes proposals, and closes deals.

## Owns
- Lead generation and qualification
- CRM automation and pipeline management
- Outreach campaigns and follow-up sequencing
- Proposal writing and deal documentation
- Win/loss analysis and pipeline health

## Does Not Own
- Budget allocation (`cfo`)
- Final contract legal terms (`clo`)
- Brand/campaign strategy (`cmo` — CRO executes pipeline, CMO builds the funnel)

## Reports To
`ceo`

## Scripts

| Script | Purpose |
|---|---|
| `pipeline_analyzer.py` | Analyze pipeline health, flag stalled deals, compute conversion rates |
| `proposal_generator.py` | Generate structured proposals from deal data and templates |

## References
- `references/sales-methodology.md` — Pipeline management and deal qualification

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/cro/<uuid>/`. See
`../../../pmcro-legacy/skills/orchestrator-agent/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Every deal in the pipeline has: stage, value, next action, and last contact date.
2. Stalled deals (>14 days no activity) get flagged with a recommended action.
3. Proposals state the problem, the solution, the timeline, and the investment.
4. Win/loss analysis names the specific factor — "price" alone isn't enough.

## Workflow

This section contains the executable workflows formerly in commands/.

### pipeline
Pipeline or sales-methodology action. Usage: /cro:pipeline <action>

---
description: "Pipeline or sales-methodology action. Usage: /cro:pipeline <action>"
---
# /cro:pipeline

```
action: <first argument>
repo_path: <the target repo root>
```

Sales methodology and pipeline movement under CRO authority.

## Steps

1. Confirm the action is sales / pipeline work (CRO Owns).
2. Run `/orchestrator:run-cycle cro "pipeline: <action>"`.
3. Output must name stage, next concrete action, and any blocker that belongs to another domain.
4. Revenue forecasts that affect cash position must be coordinated with CFO.

## Guardrails
- Never invent deal status or close dates.
- Pipeline numbers must be traceable to a source system or sealed trail.


