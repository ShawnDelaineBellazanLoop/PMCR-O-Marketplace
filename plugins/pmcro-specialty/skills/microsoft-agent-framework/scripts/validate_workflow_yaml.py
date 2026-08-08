#!/usr/bin/env python3
"""
Structural validator for a declarative MAF workflow YAML in this skill's
assets/ folder. Checks internal consistency only -- it does NOT validate
against MAF's actual runtime schema (no live package dependency), so a
clean pass here means "the graph is well-formed," not "MAF will accept
this file." Pair with a real schema check before trusting it in prod.

Usage: python3 validate_workflow_yaml.py <path-to-workflow.yaml>
"""

import sys
import yaml


def validate(path: str) -> list[str]:
    errors: list[str] = []
    with open(path, "r", encoding="utf-8") as f:
        doc = yaml.safe_load(f)

    wf = doc.get("workflow", {})
    executor_ids = {e["id"] for e in wf.get("executors", [])}

    entry = wf.get("entry_point")
    if entry not in executor_ids:
        errors.append(f"entry_point '{entry}' is not a declared executor id")

    for edge in wf.get("edges", []):
        for key in ("from", "to"):
            if edge.get(key) not in executor_ids:
                errors.append(
                    f"edge references unknown executor id in '{key}': "
                    f"{edge.get(key)!r} (edge: {edge})"
                )

    for gate in wf.get("hil_gates", []):
        if gate.get("executor") not in executor_ids:
            errors.append(
                f"hil_gates references unknown executor id: "
                f"{gate.get('executor')!r}"
            )

    return errors


def main() -> int:
    if len(sys.argv) != 2:
        print(__doc__)
        return 2

    problems = validate(sys.argv[1])
    if problems:
        print(f"FAILED -- {len(problems)} issue(s):")
        for p in problems:
            print(f"  - {p}")
        return 1

    print("OK -- workflow graph is internally consistent.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
