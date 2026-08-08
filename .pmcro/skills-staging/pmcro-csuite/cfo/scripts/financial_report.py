#!/usr/bin/env python3
"""
financial_report.py — Structured Financial Report Generator

Takes analysis results (from variance_analysis, cashflow, forecast scripts)
and produces a structured report suitable for investor updates, board decks,
or internal financial reviews.

INPUT (JSON via stdin or --file):
{
    "report_type": "investor_update" | "board_deck" | "internal_review",
    "period": "2025-Q2",
    "currency": "USD",
    "narrative_focus": "Cash position update following Series A close",
    "sections": {
        "variance": { ... output from variance_analysis.py ... },
        "cashflow": { ... output from cashflow.py ... },
        "forecast": { ... output from forecast.py ... }
    },
    "highlights_override": ["Optional custom highlights"]
}

OUTPUT (JSON to stdout):
{
    "report_type": "investor_update",
    "title": "...",
    "executive_summary": "...",
    "key_metrics": {...},
    "sections": [
        {"heading": "...", "content": "...", "figures": [...]},
        ...
    ],
    "risks_and_concerns": [...],
    "forward_look": "..."
}
"""

import sys
import json
import argparse
from typing import Any


def format_currency(amount: float, currency: str) -> str:
    """Format a currency amount for readability."""
    if abs(amount) >= 1_000_000:
        return f"{currency} {amount/1_000_000:,.1f}M"
    if abs(amount) >= 1_000:
        return f"{currency} {amount/1_000:,.0f}K"
    return f"{currency} {amount:,.0f}"


def build_executive_summary(
    report_type: str,
    narrative: str,
    sections: dict[str, Any],
) -> str:
    """Generate an executive summary from section data."""
    lines = [narrative, ""]

    # Cash position
    cf = sections.get("cashflow", {})
    if cf:
        ending = cf.get("ending_cash", 0)
        runway = cf.get("runway_status", "unknown")
        lines.append(
            f"Cash position: {format_currency(ending, cf.get('currency', 'USD'))} "
            f"({runway} runway)"
        )

    # Budget status
    var = sections.get("variance", {})
    if var:
        tv = var.get("total_variance_pct", 0)
        direction = "over" if tv > 0 else "under"
        lines.append(
            f"Budget: {abs(tv)}% {direction} budget "
            f"({format_currency(abs(var.get('total_variance', 0)), var.get('currency', 'USD'))})"
        )

    # Forecast
    fc = sections.get("forecast", {})
    if fc:
        base = fc.get("scenarios", {}).get("base", {}).get("totals", {})
        net = base.get("total_net_income", 0)
        lines.append(
            f"Forecast: {format_currency(net, fc.get('currency', 'USD'))} "
            f"net income ({fc.get('assumptions_summary', {}).get('confidence', '?')} confidence)"
        )

    return "\n".join(lines)


def build_metrics(sections: dict[str, Any]) -> dict[str, Any]:
    """Extract key metrics from all sections."""
    metrics: dict[str, Any] = {}

    cf = sections.get("cashflow", {})
    if cf:
        metrics["ending_cash"] = cf.get("ending_cash")
        metrics["burn_rate"] = cf.get("burn_rate")
        metrics["runway_months"] = cf.get("runway_months")
        metrics["runway_status"] = cf.get("runway_status")

    var = sections.get("variance", {})
    if var:
        metrics["total_variance_pct"] = var.get("total_variance_pct")
        metrics["total_budget"] = var.get("total_budget")
        metrics["total_actual"] = var.get("total_actual")
        # Find critical categories
        critical = [
            c["name"] for c in var.get("categories", [])
            if c.get("threshold") == "critical"
        ]
        if critical:
            metrics["critical_categories"] = critical

    fc = sections.get("forecast", {})
    if fc:
        base = fc.get("scenarios", {}).get("base", {}).get("totals", {})
        metrics["forecast_revenue"] = base.get("total_revenue")
        metrics["forecast_net_income"] = base.get("total_net_income")
        metrics["forecast_confidence"] = fc.get(
            "assumptions_summary", {}
        ).get("confidence")

    return metrics


def collect_risks(
    sections: dict[str, Any], metrics: dict[str, Any]
) -> list[str]:
    """Collect risks and concerns from all sections."""
    risks: list[str] = []

    # From cashflow warnings
    cf = sections.get("cashflow", {})
    for w in cf.get("warnings", []):
        risks.append(w)

    # From variance warnings
    var = sections.get("variance", {})
    for w in var.get("warnings", []):
        risks.append(w)

    # From forecast warnings
    fc = sections.get("forecast", {})
    for w in fc.get("warnings", []):
        risks.append(w)

    # Runway concern
    if metrics.get("runway_status") in ("critical", "caution"):
        risks.append(
            f"Runway at {metrics.get('runway_months', '?')} months — "
            "active funding or cost-reduction planning advised"
        )

    return risks if risks else ["No material risks identified this period"]


def build_sections(
    report_type: str,
    sections_data: dict[str, Any],
    metrics: dict[str, Any],
) -> list[dict[str, Any]]:
    """Build report sections with content and figures."""
    sections: list[dict[str, Any]] = []
    currency = sections_data.get("variance", {}).get("currency", "USD")

    # Cash Flow section
    cf = sections_data.get("cashflow", {})
    if cf:
        sections.append({
            "heading": "Cash Position",
            "content": (
                f"Ended {cf.get('period')} with "
                f"{format_currency(cf.get('ending_cash', 0), currency)} "
                f"in cash. Net cash flow: "
                f"{format_currency(cf.get('net_cash_flow', 0), currency)}. "
                f"Runway status: {cf.get('runway_status', 'N/A')}."
            ),
            "figures": [
                {"label": "Beginning Cash", "value": format_currency(cf.get("beginning_cash", 0), currency)},
                {"label": "Total Inflows", "value": format_currency(cf.get("total_inflows", 0), currency)},
                {"label": "Total Outflows", "value": format_currency(cf.get("total_outflows", 0), currency)},
                {"label": "Net Cash Flow", "value": format_currency(cf.get("net_cash_flow", 0), currency)},
                {"label": "Burn Rate", "value": format_currency(cf.get("burn_rate", 0), currency) + "/mo"},
                {"label": "Runway", "value": f"{cf.get('runway_months', 'N/A')} months"},
            ],
        })

    # Budget Variance section
    var = sections_data.get("variance", {})
    if var:
        total_var = var.get("total_variance", 0)
        direction = "over" if total_var > 0 else "under"
        sections.append({
            "heading": "Budget vs. Actual",
            "content": (
                f"Total {abs(var.get('total_variance_pct', 0))}% {direction} "
                f"budget for {var.get('period')}. "
                f"Real overruns: {', '.join(var.get('summary', {}).get('real_overruns', ['none']))}. "
                f"Timing variances: {', '.join(var.get('summary', {}).get('timing', ['none']))}."
            ),
            "figures": [
                {"label": "Total Budget", "value": format_currency(var.get("total_budget", 0), currency)},
                {"label": "Total Actual", "value": format_currency(var.get("total_actual", 0), currency)},
                {"label": "Variance", "value": f"{format_currency(abs(var.get('total_variance', 0)), currency)} ({var.get('total_variance_pct', 0):+.1f}%)"},
            ],
        })

    # Forecast section
    fc = sections_data.get("forecast", {})
    if fc:
        base = fc.get("scenarios", {}).get("base", {}).get("totals", {})
        spread = fc.get("scenario_spread", {}).get("net_income_range", {})
        sections.append({
            "heading": "Forward Look",
            "content": (
                f"Base case projects {format_currency(base.get('total_net_income', 0), currency)} "
                f"net income with {fc.get('assumptions_summary', {}).get('confidence', '?')} "
                f"confidence. Key sensitivity: "
                f"{fc.get('assumptions_summary', {}).get('key_sensitivity', 'N/A')} "
                f"({fc.get('assumptions_summary', {}).get('sensitivity_range', 'N/A')})."
            ),
            "figures": [
                {"label": "Projected Revenue", "value": format_currency(base.get("total_revenue", 0), currency)},
                {"label": "Projected Net Income", "value": format_currency(base.get("total_net_income", 0), currency)},
                {"label": "Conservative Case", "value": format_currency(spread.get("low", 0), currency)},
                {"label": "Optimistic Case", "value": format_currency(spread.get("high", 0), currency)},
            ],
        })

    return sections


def analyze(input_data: dict[str, Any]) -> dict[str, Any]:
    report_type = input_data.get("report_type", "internal_review")
    sections_data = input_data.get("sections", {})
    narrative = input_data.get("narrative_focus", "")
    highlights = input_data.get("highlights_override", None)

    metrics = build_metrics(sections_data)
    risks = collect_risks(sections_data, metrics)
    summary = build_executive_summary(report_type, narrative, sections_data)
    sections = build_sections(report_type, sections_data, metrics)

    titles = {
        "investor_update": f"Investor Update — {input_data.get('period', '')}",
        "board_deck": f"Board Financial Review — {input_data.get('period', '')}",
        "internal_review": f"Internal Financial Review — {input_data.get('period', '')}",
    }

    forward_look = ""
    fc = sections_data.get("forecast", {})
    if fc:
        summary_data = fc.get("assumptions_summary", {})
        forward_look = (
            f"Next period forecast: {summary_data.get('confidence', '?')} confidence. "
            f"Watch: {summary_data.get('key_sensitivity', 'key assumptions')}. "
            f"Sensitivity range: {summary_data.get('sensitivity_range', 'N/A')}."
        )

    return {
        "report_type": report_type,
        "title": titles.get(report_type, titles["internal_review"]),
        "period": input_data.get("period", "unspecified"),
        "currency": input_data.get("currency", "USD"),
        "executive_summary": summary,
        "key_metrics": metrics,
        "sections": sections,
        "highlights": (
            highlights
            if highlights
            else [
                f"Cash: {format_currency(metrics.get('ending_cash', 0), input_data.get('currency', 'USD'))}",
                f"Burn: {format_currency(metrics.get('burn_rate', 0), input_data.get('currency', 'USD'))}/mo",
                f"Runway: {metrics.get('runway_months', 'N/A')} months",
            ]
        ),
        "risks_and_concerns": risks,
        "forward_look": forward_look,
    }


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Structured Financial Report Generator"
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
