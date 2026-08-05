# GAAP Reference — CFO Domain

Core principles from US Generally Accepted Accounting Principles relevant to
financial analysis performed by this skill. This is a working reference, not
a comprehensive restatement.

## Principles That Govern Analysis

### Revenue Recognition (ASC 606)
- Recognize revenue when control transfers to the customer, not when cash arrives.
- For subscription/MRR: recognize ratably over the service period.
- For one-time sales: recognize at delivery.
- **Implication for forecasts**: projected revenue ≠ projected cash receipts.
  Always separate the two in cash-flow analysis.

### Expense Matching
- Match expenses to the period they benefit, not the period they're paid.
- Prepaid expenses (annual licenses, insurance) spread across months.
- Accrued expenses (services received, not yet invoiced) recognized when incurred.
- **Implication for variance analysis**: a "favorable" variance from delayed
  invoicing is a timing artifact, not a real saving.

### Materiality
- An error or omission is material if it would change a reasonable person's
  judgment.
- **Implication for reporting**: flag items that cross 5% of the relevant
  baseline (5% of total budget, 5% of cash position, etc.). Don't drown
  readers in immaterial line items.

### Consistency
- Apply the same methods period to period. If a method changes, disclose it.
- **Implication for forecasts**: if you switch from top-down to bottom-up
  modeling mid-stream, state the switch and explain why.

### Conservatism
- When two estimates are equally reasonable, choose the one less favorable
  to the company's position.
- **Implication for projections**: don't round up. The forecast-modeler's
  rule — "flag the single assumption the forecast is most sensitive to" —
  derives from this principle.

## Classification Conventions

### Operating vs. Capital
- **Operating expense (OpEx)**: consumed within the period (salaries, rent,
  cloud hosting, marketing).
- **Capital expenditure (CapEx)**: creates an asset with useful life beyond
  the period (equipment, capitalized software development).
- **Implication for cash flow**: CapEx hits the balance sheet first, then
  depreciates. Cash outlay and P&L impact happen in different periods.

### Fixed vs. Variable
- **Fixed**: doesn't scale with output/revenue (rent, base salaries,
  minimum infrastructure).
- **Variable**: scales with output/revenue (cloud compute, transaction fees,
  commission).
- **Implication for forecasting**: variable costs should be modeled as a
  function of the revenue/demand assumption, not as a flat number.

## Key Ratios

| Ratio | Formula | What It Tells You |
|---|---|---|
| Gross Margin | (Revenue - COGS) / Revenue | Unit economics health |
| Operating Margin | Operating Income / Revenue | Core business profitability |
| Burn Rate | Net Cash Outflow / Month | How fast you're spending |
| Runway (months) | Cash / Burn Rate | How long until cash runs out |
| Budget Variance % | (Actual - Budget) / Budget | Where you're off plan |
| Quick Ratio | (Cash + Receivables) / Current Liabilities | Can you pay short-term obligations |

## When GAAP Doesn't Apply

- Early-stage startups often report on a cash basis, not accrual. Note which
  basis you're using.
- Internal management reporting may use non-GAAP metrics (EBITDA, ARR, MRR)
  as long as they're clearly labeled as such.
- Investor updates are not GAAP financial statements. They can use simplified
  metrics but should note deviations from formal accounting.
