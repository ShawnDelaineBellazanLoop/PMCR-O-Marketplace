# Forecasting Reference — CFO Domain

Assumption frameworks and best practices for financial projections.

## The Forecast Contract

Every forecast must state, before the numbers:

1. **Time horizon** — what period does this cover?
2. **Growth rate assumption** — what rate is applied to what baseline?
3. **Cost basis** — fixed costs, variable costs as % of revenue, step costs
4. **Key sensitivity** — the single assumption this forecast is most sensitive to
5. **Confidence level** — high/medium/low, and why

A forecast that doesn't declare its assumptions is a guessing game, not analysis.

## Projection Methods

### Top-Down
Start from market size → addressable market → market share → revenue.
- **Best for**: early-stage, pre-revenue, market-sizing exercises.
- **Weakness**: market-share assumptions are often wishful thinking.

### Bottom-Up
Start from unit economics → customer count → revenue.
- **Best for**: operational companies with real customer data.
- **Weakness**: bottoms-up optimism (every deal closes, every hire starts on time).

### Trend Extrapolation
Fit a model to historical data, project forward.
- **Best for**: stable, mature operations with reliable history.
- **Weakness**: assumes the future looks like the past.

State which method you're using and why it fits the context.

## Scenario Modeling

For any forecast with medium or low confidence, produce three scenarios:

| Scenario | Growth Rate | Cost Growth | Description |
|---|---|---|---|
| Conservative | Base - 30% | Base + 10% | Downside case |
| Base | Best estimate | Best estimate | Most likely |
| Optimistic | Base + 30% | Base - 10% | Upside case |

The gap between Conservative and Optimistic *is* the uncertainty range.
Don't present a single number when the range matters more than the midpoint.

## Cash Flow Specifics

### Inflows
- Operating cash receipts (revenue collected, not just recognized)
- Financing (investment, loans)
- One-time inflows (tax refunds, asset sales)

### Outflows
- Operating cash payments (payroll, vendors, rent)
- Capital expenditures
- Debt service (interest + principal)
- One-time outflows (bonuses, legal settlements)

### Runway Calculation
```
Runway (months) = Cash Position / Net Monthly Burn

where Net Monthly Burn = Average(Outflows - Inflows) over the period
```

Runway under 6 months: critical — flag immediately.
Runway 6-12 months: caution — recommend funding planning.
Runway 12+ months: healthy — note in passing.

## Common Forecasting Errors

1. **Hockey-stick projections**: "flat this year, 3x next year" with no structural
   change to justify the inflection. Call these out explicitly.
2. **Costs that don't scale with revenue**: if you project 3x revenue growth but
   flat headcount, explain how. Otherwise, model headcount growth.
3. **Ignoring step costs**: cloud infrastructure doesn't scale linearly — there
   are pricing tiers. Hiring doesn't scale continuously — there are recruiting
   cycles and ramp time.
4. **Single-point forecasts without ranges**: a forecast with no error bars is a
   point estimate pretending to be certain. Always include the sensitivity range.
