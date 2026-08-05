#!/usr/bin/env python3
"""
cashflow.py — Cash Flow Position Analysis

Analyzes cash inflows/outflows, computes net position, burn rate, and runway.
Flags critical conditions (runway < 6 months, negative net cash flow trend).

INPUT (JSON via stdin or --file):
{
    "period": "2025-06",
    "currency": "USD",
    "beginning_cash": 500000,
    "inflows": {
        "operating": [{"category": "Customer Receipts", "amount": 120000}],
        "financing": [{"category": "Investment", "amount": 0}],
        "one_time": [{"category": "Tax Refund", "amount": 5000}]
    },
    "outflows": {
        "operating": [
            {"category": "Payroll", "amount": 95000},
            {"category": "Cloud", "amount": 22000},
            {"category": "Marketing", "amount": 15000}
        ],
        "capital": [{"category": "Equipment", "amount": 8000}],
        "debt_service": [{"category": "Loan Payment", "amount": 5000}],
        "one_time": [{"category": "Legal Settlement", "amount": 0}]
    },
    "prior_months_burn": [45000, 52000, 48000]  // optional, for trend
}

OUTPUT (JSON to stdout):
{
    "period": "2025-06",
    "beginning_cash": 500000,
    "total_inflows": ...,
    "total_outflows": ...,
    "net_cash_flow": ...,
    "ending_cash": ...,
    "burn_rate": ...,
    "runway_months": ...,
    "runway_status": "healthy" | "caution" | "critical",
    "inflow_breakdown": {...},
    "outflow_breakdown": {...},
    "warnings": [...]
}
"""

import sys
import json
import argparse
from typing import Any


def sum_category(items: list[dict[str, Any]]) -> float:
    return sum(float(item["amount"]) for item in items)


def classify_runway(months: float) -> str:
    if months < 0:
        return "critical"  # negative cash flow
    if months < 6:
        return "critical"
    if months < 12:
        return "caution"
    return "healthy"


def analyze(input_data: dict[str, Any]) -> dict[str, Any]:
    inflows = input_data.get("inflows", {})
    outflows = input_data.get("outflows", {})
    beginning_cash = float(input_data.get("beginning_cash", 0))

    total_inflows = (
        sum_category(inflows.get("operating", []))
        + sum_category(inflows.get("financing", []))
        + sum_category(inflows.get("one_time", []))
    )
    total_outflows = (
        sum_category(outflows.get("operating", []))
        + sum_category(outflows.get("capital", []))
        + sum_category(outflows.get("debt_service", []))
        + sum_category(outflows.get("one_time", []))
    )

    net_cash_flow = total_inflows - total_outflows
    ending_cash = beginning_cash + net_cash_flow
    burn_rate = -net_cash_flow if net_cash_flow < 0 else 0.0

    # Runway: months until cash runs out at current burn rate
    if burn_rate > 0:
        runway_months = round(ending_cash / burn_rate, 1)
    elif ending_cash > 0:
        runway_months = float("inf")  # positive or neutral cash flow
    else:
        runway_months = 0.0

    runway_status = classify_runway(runway_months)

    # Build breakdowns
    inflow_breakdown: dict[str, float] = {}
    for key, items in inflows.items():
        inflow_breakdown[key] = round(sum_category(items), 2)

    outflow_breakdown: dict[str, float] = {}
    for key, items in outflows.items():
        outflow_breakdown[key] = round(sum_category(items), 2)

    # Per-category detail
    outflow_detail: dict[str, float] = {}
    for items in outflows.values():
        for item in items:
            outflow_detail[item["category"]] = float(item["amount"])

    # Trend check
    warnings: list[str] = []
    prior = input_data.get("prior_months_burn", [])
    if len(prior) >= 2 and burn_rate > 0:
        recent = prior[-2:] + [burn_rate]
        if recent[-1] > recent[-2] > recent[-3]:
            warnings.append(
                "Burn rate trending upward over last 3 periods — "
                f"{prior[-2]:.0f} → {prior[-1]:.0f} → {burn_rate:.0f}"
            )

    if runway_status == "critical":
        warnings.append(
            f"CRITICAL: {runway_months:.1f} months runway at current burn rate"
        )
    elif runway_status == "caution":
        warnings.append(
            f"CAUTION: {runway_months:.1f} months runway — "
            "funding planning recommended"
        )

    # Top outflow categories
    sorted_outflows = sorted(
        outflow_detail.items(), key=lambda x: x[1], reverse=True
    )
    top_outflows = sorted_outflows[:3]

    return {
        "period": input_data.get("period", "unspecified"),
        "currency": input_data.get("currency", "USD"),
        "beginning_cash": round(beginning_cash, 2),
        "total_inflows": round(total_inflows, 2),
        "total_outflows": round(total_outflows, 2),
        "net_cash_flow": round(net_cash_flow, 2),
        "ending_cash": round(ending_cash, 2),
        "burn_rate": round(burn_rate, 2),
        "runway_months": (
            runway_months
            if isinstance(runway_months, float) and runway_months != float("inf")
            else None
        ),
        "runway_status": runway_status,
        "inflow_breakdown": inflow_breakdown,
        "outflow_breakdown": outflow_breakdown,
        "outflow_detail_top3": [
            {"category": cat, "amount": amt}
            for cat, amt in top_outflows
        ],
        "warnings": warnings,
    }


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Cash Flow Position Analysis"
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