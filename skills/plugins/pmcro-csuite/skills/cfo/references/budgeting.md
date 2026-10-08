# Budgeting Reference — CFO Domain

Methodology, variance categories, and analysis patterns for budget work.

## Budget Structure

A budget is a financial plan allocating resources across categories for a
defined period. Budgets this skill analyzes typically follow this shape:

```
Category          Monthly Budget    YTD Budget    YTD Actual    Variance
─────────────────────────────────────────────────────────────────────────
Engineering       $X                $Y            $Z            $Δ (p%)
Marketing         ...
Sales             ...
G&A               ...
Cloud/Infra       ...
─────────────────────────────────────────────────────────────────────────
TOTAL             $ΣX               $ΣY           $ΣZ           $ΣΔ
```

## Variance Categories

When analyzing budget vs. actual, classify each variance:

| Category | Definition | Example |
|---|---|---|
| **Timing** | Spend happened in a different period than planned | License renewed in March instead of January — evens out over the year |
| **Real Overrun** | Actual spend exceeds planned, won't self-correct | New hire started at higher salary than budgeted |
| **Real Underrun** | Actual spend below planned, sustainable | Negotiated a lower vendor rate |
| **Scope Change** | Budget assumption no longer matches reality | Planned headcount of 5, actually hired 3 — variance is structural |

Timing variances do not require corrective action. Real overruns do. Scope
changes require a budget revision, not a variance explanation.

## Overrun Thresholds

Not all variances need escalation. Use these thresholds:

| Variance % | Label | Action |
|---|---|---|
| < 5% | Immaterial | Note, no escalation |
| 5-15% | Notable | Flag with category + cause |
| 15-30% | Significant | Flag + recommend corrective action |
| > 30% | Critical | Flag + recommend budget revision |

These thresholds apply per category, not just to the total. A 4% total
variance that masks a 40% overrun in one category is a reporting failure.

## Budgeting Methodologies

### Zero-Based Budgeting (ZBB)
Every dollar must be justified each period. No "last year + 10%."
- **Best for**: cost optimization cycles, turnaround situations.
- **Weakness**: time-intensive, can undervalue ongoing operations.

### Incremental Budgeting
Start from prior period actuals, adjust for known changes.
- **Best for**: stable operations, maintenance-mode budgets.
- **Weakness**: bakes in prior inefficiencies.

### Driver-Based Budgeting
Budget is a function of business drivers (headcount, ARR, customers).
- **Best for**: growth-stage companies where scale changes assumptions.
- **Weakness**: driver forecasts can be as uncertain as budget line items.

When a cycle's `true_intent` is budget-related, state which methodology
applies to the analysis. If the methodology isn't specified by the user,
default to driver-based for growth-stage contexts and incremental for
stable contexts.

## Cost Optimization Patterns

When analyzing for cost reduction, work through these lenses in order:

1. **Elimination**: can the spend be stopped entirely without material harm?
2. **Reduction**: can the same outcome be achieved with less?
3. **Substitution**: can a cheaper alternative deliver the same result?
4. **Renegotiation**: can the vendor/supplier terms be improved?
5. **Deferral**: can the spend be moved to a later period?

Work through them in sequence — don't jump to renegotiation when
elimination is the honest answer.
