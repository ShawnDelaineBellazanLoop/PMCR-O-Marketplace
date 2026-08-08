---
description: "Build conservative / base / optimistic financial forecast. Usage: /cfo:forecast <horizon>"
---
# /cfo:forecast

```
horizon: <first argument – e.g. "Q3-Q4 2026" or "6 months">
repo_path: <the target repo root>
```

Financial projection builder producing three scenarios.

## Steps

1. Require explicit assumptions before any numbers:
   - time horizon
   - growth-rate assumption
   - cost basis (fixed / variable / step)
   - key sensitivity
   - confidence level (high / medium / low)
2. Dispatch `/orchestrator:run-cycle cfo "forecast for <horizon>"`.
3. Output must contain conservative / base / optimistic scenarios and the sensitivity range.
4. Call out hockey-stick projections when total growth is high and confidence is not high.

## Guardrails
- A forecast without declared assumptions is invalid.
- Prefer driver-based or bottom-up methods when real operational data exists.
- Never present a single-point forecast when the range matters more than the midpoint.
