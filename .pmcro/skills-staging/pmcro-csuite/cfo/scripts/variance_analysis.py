#!/usr/bin/env python3
"""
variance_analysis.py — Budget vs. Actual Variance Analysis

Compares budgeted amounts against actuals, computes variances by category,
and classifies each variance as timing/real/scope-change per CFO methodology.

INPUT (JSON via stdin or --file):
{
    "period": "2025-Q2",
    "currency": "USD",
    "categories": [
        {
            "name": "Engineering",
            "budget": 150000,
            "actual": 162000,
            "notes": "One new hire started mid-quarter"
        },
        ...
    ]
}

OUTPUT (JSON to stdout):
{
    "period": "2025-Q2",
    "total_budget": ...,
    "total_actual": ...,
    "total_variance": ...,
    "total_variance_pct": ...,
    "categories": [
        {
            "name": "Engineering",
            "budget": 150000,
            "actual": 162000,
            "variance": 12000,
            "variance_pct": 8.0,
            "classification": "real_overrun",
            "threshold": "notable"
        },
        ...
    ],
    "summary": {
        "real_overruns": [...],
        "real_underruns": [...],
        "timing": [...],
        "scope_changes": [...]
    },
    "warnings": ["Category X exceeds 30% variance — critical"]
}
"""

import sys
import json
import argparse
from typing import Any


def classify_variance(
    variance_pct: float, notes: str
) -> tuple[str, str]:
    """Classify a variance by type and threshold."""
    # Classification
    notes_lower = notes.lower()
    if any(w in notes_lower for w in ["timing", "delayed", "early", "shifted"]):
        classification = "timing"
    elif any(w in notes_lower for w in [
        "scope", "headcount change", "plan change", "revision"
    ]):
        classification = "scope_change"
    elif variance_pct > 0:
        classification = "real_overrun"
    elif variance_pct < 0:
        classification = "real_underrun"
    else:
        classification = "on_budget"

    # Threshold
    abs_pct = abs(variance_pct)
    if abs_pct < 5:
        threshold = "immaterial"
    elif abs_pct < 15:
        threshold = "notable"
    elif abs_pct < 30:
        threshold = "significant"
    else:
        threshold = "critical"

    return classification, threshold


def analyze(input_data: dict[str, Any]) -> dict[str, Any]:
    """Run variance analysis on the input data."""
    categories = input_data.get("categories", [])
    results: list[dict[str, Any]] = []
    total_budget = 0.0
    total_actual = 0.0

    for cat in categories:
        budget = float(cat["budget"])
        actual = float(cat["actual"])
        variance = actual - budget
        variance_pct = round((variance / budget) * 100, 1) if budget != 0 else 0.0

        classification, threshold = classify_variance(
            variance_pct, cat.get("notes", "")
        )

        total_budget += budget
        total_actual += actual

        results.append({
            "name": cat["name"],
            "budget": budget,
            "actual": actual,
            "variance": round(variance, 2),
            "variance_pct": variance_pct,
            "classification": classification,
            "threshold": threshold,
            "notes": cat.get("notes", ""),
        })

    total_variance = total_actual - total_budget
    total_variance_pct = (
        round((total_variance / total_budget) * 100, 1)
        if total_budget != 0 else 0.0
    )

    # Build summary buckets — map classification values to display keys
    classification_to_key = {
        "real_overrun": "real_overruns",
        "real_underrun": "real_underruns",
        "timing": "timing",
        "scope_change": "scope_changes",
        "on_budget": "on_budget",
    }
    summary: dict[str, list[str]] = {
        "real_overruns": [],
        "real_underruns": [],
        "timing": [],
        "scope_changes": [],
        "on_budget": [],
    }
    for r in results:
        key = classification_to_key.get(r["classification"], r["classification"])
        summary[key].append(r["name"])

    # Build warnings
    warnings: list[str] = []
    for r in results:
        if r["threshold"] == "critical":
            warnings.append(
                f"{r['name']}: {r['variance_pct']:+.1f}% variance "
                f"({r['classification']}) — critical threshold exceeded"
            )
        elif r["threshold"] == "significant" and r["classification"] in (
            "real_overrun", "real_underrun"
        ):
            warnings.append(
                f"{r['name']}: {r['variance_pct']:+.1f}% variance "
                f"({r['classification']}) — significant, review recommended"
            )

    return {
        "period": input_data.get("period", "unspecified"),
        "currency": input_data.get("currency", "USD"),
        "total_budget": round(total_budget, 2),
        "total_actual": round(total_actual, 2),
        "total_variance": round(total_variance, 2),
        "total_variance_pct": total_variance_pct,
        "categories": results,
        "summary": summary,
        "warnings": warnings,
    }


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Budget vs. Actual Variance Analysis"
    )
    parser.add_argument(
        "--file", type=str, help="Path to JSON input file"
    )
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
