---
name: property-preservation
description: "Property Preservation domain skill — Tooensure Recovery Services field/contractor execution: work orders, bid writing, inspection reports, photo analysis, compliance, vendor coordination, and county data extraction."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=property-preservation."
pattern_d: opt-in
---

# Property Preservation Domain Skill

Industry-specific execution specialist for Tooensure Recovery Services'
Ramsey County property preservation business. Handles the field/contractor
side of the business: work order lifecycle, inspection reporting, bid
writing, vendor coordination, and county-record research used to find and
qualify vacant/ghost properties.

## Owns
- Work order creation, tracking, and closeout for contractor jobs
- Bid writing for property preservation contracts
- Field inspection reporting (photo analysis, condition documentation)
- Vendor/contractor coordination for Ramsey County property work
- County-record research: shadow/vacant property identification via
  Beacon GIS portal automation (tax delinquency, owner address mismatch,
  estate/trust ownership signals)
- Compliance tracking against county property-preservation standards

## Does Not Own
- Company-wide SOP design (`coo` — this domain executes within COO's
  operational framework)
- Financial reporting (`cfo`)
- Contract legal terms (`clo`)
- Chat/session distillation, pattern extraction, memory hydration
  (`domain-specialist` — a separate skill, do not conflate)

## Reports To
`coo`

## Scripts

| Script | Purpose |
|---|---|
| `beacon_scraper.py` | Automate Ramsey County Beacon GIS portal to extract vacant/shadow property records |
| `ghost_property_scorer.py` | Score extracted properties on tax delinquency, address mismatch, and estate/trust signals to rank preservation candidates |
| `inspection_report_builder.py` | Assemble field inspection photos and notes into a structured report |

## References
- `references/ramsey-county-workflow.md` — Beacon scraper methodology and ghost-property qualification criteria

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/property-preservation/<uuid>/`. See
`skills/orchestrator-agent/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements — this skill does not restate
them. Mid-plan consults are unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Never fabricate county-record data — every extracted property cites its
   Beacon source record; if the scraper returns nothing, say so.
2. Ghost-property scoring requires evidence from 2+ signals (delinquency +
   address mismatch, or delinquency + estate/trust ownership) — a single
   signal alone is not a qualifying flag.
3. Field inspection reports separate: observed condition, contractor
   recommendation, and compliance status — do not blend them into one note.
4. This skill does not own chat/session distillation or memory hydration —
   route that work to `domain-specialist` instead of duplicating it here.
