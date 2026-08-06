---
name: cmo
description: "CMO domain skill — content creation, social media, campaign management, SEO, and brand voice consistency."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=cmo."
metadata:
  pattern_d: opt-in
---

# CMO Domain Skill

Builds the brand. Creates content, manages social media, runs campaigns,
optimizes SEO, and maintains a consistent brand voice.

## Owns
- Content creation and editorial strategy
- SEO optimization and keyword strategy
- Social media management and scheduling
- Campaign management and performance analysis
- Brand voice definition and consistency enforcement

## Does Not Own
- Lead/deal pipeline ownership (`cro`)
- Legal review of marketing claims (`clo`)
- Company-wide OKRs (`ceo`)

## Reports To
`ceo`

## Scripts

| Script | Purpose |
|---|---|
| `content_analyzer.py` | Score content readability, keyword density, brand voice alignment |
| `campaign_metrics.py` | Compute campaign ROI, engagement rates, conversion attribution |

## References
- `references/marketing-strategy.md` — Campaign design and content frameworks

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/cmo/<uuid>/`. See
`../../../pmcro-legacy/skills/orchestrator-agent/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Every piece of content states its audience, channel, and call to action.
2. Brand voice violations get flagged with the specific phrase and the alternative.
3. Campaign analysis separates leading indicators (engagement) from lagging (revenue).
4. SEO recommendations cite the specific keyword, volume, and competition level.

## Workflow

This section contains the executable workflows formerly in commands/.

### campaign
Produce or update a marketing strategy artifact. Usage: /cmo:campaign <name>

---
description: "Produce or update a marketing strategy artifact. Usage: /cmo:campaign <name>"
---
# /cmo:campaign

```
name: <first argument>
repo_path: <the target repo root>
```

Marketing strategy and campaign definition under CMO authority.

## Steps

1. Confirm the request is marketing strategy (CMO Owns).
2. Dispatch `/orchestrator:run-cycle cmo "campaign or strategy: <name>"`.
3. Output must include audience, channel, message, success metric, and owner.
4. Budget implications must be handed to CFO; technical delivery implications to CTO/COO.

## Guardrails
- Do not invent performance numbers.
- Keep claims falsifiable so Checker can evaluate them later.


