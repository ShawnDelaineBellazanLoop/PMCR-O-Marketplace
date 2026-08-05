---
description: "Produce cash-position, burn-rate, and runway analysis. Usage: /cfo:cashflow <period>"
---
# /cfo:cashflow

```
period: <first argument – e.g. 2026-Q3 or 2026-08>
repo_path: <the target repo root>
```

Cash-flow position analysis using the Colony’s cashflow.py contract.

## Steps

1. Confirm the request is cash-position / burn / runway work (CFO Owns).
2. Invoke the `scripts/cashflow.py` input contract with the supplied period data (or previously sealed financial data).
3. Run `/orchestrator:run-cycle cfo "cash-flow analysis for <period>"`.
4. Classify runway:
   - < 6 months → CRITICAL
   - 6–12 months → CAUTION
   - 12+ months → healthy
5. Never invent inflow or outflow numbers — only analyze supplied or sealed data.

## Guardrails
- Separate operating / financing / one-time inflows and outflows.
- Flag upward burn-rate trends explicitly.
- Materiality threshold: items that change a reasonable person’s judgment must be surfaced.
