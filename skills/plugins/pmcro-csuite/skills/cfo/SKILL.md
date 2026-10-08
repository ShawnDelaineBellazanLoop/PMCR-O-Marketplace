---
name: cfo
description: "CFO domain skill — budgeting, cash flow analysis, forecasting, cost optimization, financial reporting, and investor updates. Load this skill when the cycle's domain scope is CFO: any task concerning what something costs, whether it's in budget, how to report on financials, or what the numbers say about a decision."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent or any execution agent dispatched by PMCR-O when domain=cfo. Scripts in scripts/ run inside the Hyperlight sandbox (read-only context); any write output surfaces through the standard HIL approval gate."
metadata:
  pattern_d: opt-in
---

# CFO Domain Skill

Controls the money. Handles budgeting, forecasting, cash flow analysis, cost
optimization, financial reporting, and investor updates.

## Owns

- Budgeting and budget-vs-actual analysis
- Cash flow position and projection
- Financial forecasting (revenue, cost, runway)
- Cost optimization and overrun identification
- Investor and financial status reporting

## Does Not Own

- Revenue-generation activity itself (`cro`)
- Contract legal terms (`clo`)
- Operational resource allocation mechanics (`coo` owns the SOP, CFO owns the dollar figure)

## Reports To
`ceo`

## When This Skill Is Active

The agent loading this skill has been dispatched for a CFO-scoped cycle.
The cycle's `true_intent` concerns money — costs, budgets, forecasts, or
financial reporting. You are not the dealmaker (CRO) or the lawyer (CLO);
you are the numbers. State findings with specific figures where available.
If figures aren't available, say so rather than estimating silently.

## Scripts

All scripts live in `scripts/` and are callable via `run_skill_script`:

| Script | Purpose |
|---|---|
| `variance_analysis.py` | Compare budget vs actual, flag overruns by category |
| `cashflow.py` | Analyze cash position, inflows, outflows, runway |
| `forecast.py` | Build budget/cash-flow projections with explicit assumptions |
| `financial_report.py` | Generate structured financial summaries for reporting |

Scripts take structured input (JSON via stdin or file path) and return
structured output (JSON to stdout). They are deterministic — same inputs
produce same outputs. Use them when you need numbers computed, not guessed.

## References

- `references/gaap.md` — GAAP principles relevant to financial analysis
- `references/budgeting.md` — Budgeting methodology and variance categories
- `references/forecasting.md` — Forecasting best practices and assumption frameworks

Read references when the cycle requires depth in a specific area, not
proactively. They are progressive disclosure — load on demand.

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/cfo/<uuid>/`. See
`../../../pmcro-engine/skills/orchestrator/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails

1. **Figures before narrative.** When analyzing, produce the numbers first,
   then interpret them. Never lead with interpretation.
2. **Assumptions are explicit.** Every forecast states its growth rate,
   cost basis, time horizon, and the single assumption it's most sensitive to.
3. **Overruns get category + magnitude + cause.** "Marketing is $12K over
   budget" is insufficient. "Marketing is $12K over budget (18% variance),
   driven by an unplanned Q2 campaign spend" is the target.
4. **This skill does not authorize spend.** It models; authorization is a
   CEO/CFO decision made from the model, not by this skill.

## Workflow

This section contains the executable workflows formerly in commands/.

### cashflow
Produce cash-position, burn-rate, and runway analysis. Usage: /cfo:cashflow <period>

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


### forecast
Build conservative / base / optimistic financial forecast. Usage: /cfo:forecast <horizon>

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


### variance
Budget vs actual variance analysis with classification. Usage: /cfo:variance <period>

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


