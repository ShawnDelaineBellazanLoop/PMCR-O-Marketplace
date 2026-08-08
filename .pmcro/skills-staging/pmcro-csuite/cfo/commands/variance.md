---
description: "Budget vs actual variance analysis with classification. Usage: /cfo:variance <period>"
---
# /cfo:variance

```
period: <first argument>
repo_path: <the target repo root>
```

Budget-versus-actual variance analysis.

## Steps

1. Classify every variance as one of: timing | real_overrun | real_underrun | scope_change.
2. Apply materiality thresholds per category:
   - < 5 % → immaterial
   - 5–15 % → notable
   - 15–30 % → significant
   - > 30 % → critical
3. Route through `/orchestrator:run-cycle cfo "variance analysis for <period>"`.
4. Timing variances do not require corrective action; real overruns and scope changes do.

## Guardrails
- A 4 % total variance that masks a 40 % overrun in one category is a reporting failure.
- Never invent budget or actual figures.
