#!/usr/bin/env python3
"""
forecast.py — Financial Projection Builder

Builds budget and cash-flow projections from baseline data and explicit
assumptions. Produces conservative/base/optimistic scenarios.

INPUT (JSON via stdin or --file):
{
    "label": "Q3-Q4 2025 Forecast",
    "currency": "USD",
    "periods": 6,
    "period_label": "month",
    "baseline_revenue": 100000,
    "assumptions": {
        "monthly_growth_rate": 0.05,
        "cogs_pct": 0.30,
        "fixed_costs": {
            "Payroll": 60000,
            "Rent": 8000,
            "Software": 3000
        },
        "variable_costs": {
            "Cloud": {"basis": "revenue", "rate": 0.08},
            "Marketing": {"basis": "revenue", "rate": 0.12},
            "Transaction Fees": {"basis": "revenue", "rate": 0.03}
        },
        "step_costs": {
            "New Hire - Q3": {"period": 3, "amount": 10000, "recurring": true}
        },
        "sensitivity": {
            "variable": "monthly_growth_rate",
            "conservative_delta": -0.03,
            "optimistic_delta": 0.03
        }
    }
}

OUTPUT (JSON to stdout):
{
    "label": "...",
    "assumptions_summary": {
        "growth_rate": 0.05,
        "key_sensitivity": "monthly_growth_rate",
        "sensitivity_range": "2% to 8%",
        "confidence": "medium"
    },
    "scenarios": {
        "conservative": {...},
        "base": {...},
        "optimistic": {...}
    }
}
"""

import sys
import json
import argparse
from typing import Any
from copy import deepcopy


def build_projection(
    baseline_revenue: float,
    periods: int,
    growth_rate: float,
    assumptions: dict[str, Any],
) -> dict[str, Any]:
    """Build a single scenario projection."""
    revenue = baseline_revenue
    cumulative_revenue = 0.0
    cumulative_costs = 0.0
    periods_data: list[dict[str, Any]] = []

    fixed_costs = assumptions.get("fixed_costs", {})
    fixed_monthly = sum(float(v) for v in fixed_costs.values())

    variable_costs_def = assumptions.get("variable_costs", {})
    step_costs_def = assumptions.get("step_costs", {})

    for period in range(1, periods + 1):
        # Revenue
        revenue = baseline_revenue * ((1 + growth_rate) ** (period - 1))

        # COGS
        cogs = revenue * float(assumptions.get("cogs_pct", 0))

        # Variable costs
        variable = 0.0
        for name, vc in variable_costs_def.items():
            basis_val = revenue if vc.get("basis") == "revenue" else 0
            variable += basis_val * float(vc.get("rate", 0))

        # Step costs (triggered in this period)
        step = 0.0
        for name, sc in step_costs_def.items():
            if int(sc.get("period", 0)) == period:
                step += float(sc.get("amount", 0))

        total_costs = fixed_monthly + cogs + variable + step
        gross_profit = revenue - cogs
        net_income = revenue - total_costs

        cumulative_revenue += revenue
        cumulative_costs += total_costs

        periods_data.append({
            "period": period,
            "revenue": round(revenue, 2),
            "cogs": round(cogs, 2),
            "gross_profit": round(gross_profit, 2),
            "fixed_costs": round(fixed_monthly, 2),
            "variable_costs": round(variable, 2),
            "step_costs": round(step, 2),
            "total_costs": round(total_costs, 2),
            "net_income": round(net_income, 2),
            "margin_pct": round((net_income / revenue) * 100, 1) if revenue else 0.0,
        })

    return {
        "growth_rate": growth_rate,
        "periods": periods_data,
        "totals": {
            "total_revenue": round(cumulative_revenue, 2),
            "total_costs": round(cumulative_costs, 2),
            "total_net_income": round(cumulative_revenue - cumulative_costs, 2),
            "avg_margin_pct": round(
                ((cumulative_revenue - cumulative_costs) / cumulative_revenue) * 100, 1
            ) if cumulative_revenue else 0.0,
        },
    }


def analyze(input_data: dict[str, Any]) -> dict[str, Any]:
    baseline = float(input_data["baseline_revenue"])
    periods = int(input_data["periods"])
    assumptions = input_data.get("assumptions", {})
    growth_rate = float(assumptions.get("monthly_growth_rate", 0))
    sensitivity = assumptions.get("sensitivity", {})

    # Build three scenarios
    base = build_projection(baseline, periods, growth_rate, assumptions)

    sens_var = sensitivity.get("variable", "monthly_growth_rate")
    cons_delta = float(sensitivity.get("conservative_delta", -0.03))
    opt_delta = float(sensitivity.get("optimistic_delta", 0.03))

    cons_rate = max(0.0, growth_rate + cons_delta)
    opt_rate = growth_rate + opt_delta

    conservative = build_projection(baseline, periods, cons_rate, assumptions)
    optimistic = build_projection(baseline, periods, opt_rate, assumptions)

    # Confidence assessment
    spread = opt_rate - cons_rate
    if spread < 0.03:
        confidence = "high"
    elif spread < 0.08:
        confidence = "medium"
    else:
        confidence = "low"

    # Check for hockey-stick
    last_period = base["periods"][-1]
    first_period = base["periods"][0]
    total_growth = (
        (last_period["revenue"] / first_period["revenue"] - 1)
        if first_period["revenue"] else 0
    )

    warnings: list[str] = []
    if total_growth > 2.0 and confidence in ("low", "medium"):
        warnings.append(
            f"Hockey-stick alert: {total_growth*100:.0f}% total growth "
            f"over {periods} periods with {confidence} confidence — "
            "verify the structural driver of this inflection"
        )

    return {
        "label": input_data.get("label", "unspecified"),
        "currency": input_data.get("currency", "USD"),
        "assumptions_summary": {
            "growth_rate": growth_rate,
            "cogs_pct": assumptions.get("cogs_pct", 0),
            "key_sensitivity": sens_var,
            "sensitivity_range": f"{cons_rate*100:.1f}% to {opt_rate*100:.1f}%",
            "total_growth_pct": round(total_growth * 100, 1),
            "confidence": confidence,
        },
        "scenarios": {
            "conservative": conservative,
            "base": base,
            "optimistic": optimistic,
        },
        "scenario_spread": {
            "revenue_range": {
                "low": conservative["totals"]["total_revenue"],
                "base": base["totals"]["total_revenue"],
                "high": optimistic["totals"]["total_revenue"],
            },
            "net_income_range": {
                "low": conservative["totals"]["total_net_income"],
                "base": base["totals"]["total_net_income"],
                "high": optimistic["totals"]["total_net_income"],
            },
        },
        "warnings": warnings,
    }


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Financial Projection Builder"
    )
    parser.add_argument("--file", type=str, help="Path to JSON input file")
    args = parser.parse_args()

    if args.file:
        with open(args.file, "r") as f:
            input_data = json.load(f)
    else:
        input_data = json.load(sys.stdin)

    result = analyze(input_data)
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
