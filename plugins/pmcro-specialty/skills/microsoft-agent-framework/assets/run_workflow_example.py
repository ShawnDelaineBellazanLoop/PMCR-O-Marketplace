"""
Illustrative sketch of loading and streaming the declarative
vendor-onboarding workflow via MAF's native builder + in-process
execution engine.

NOT verified against a live agent-framework-declarative install --
treat this as a shape reference for how the pieces fit together, and
confirm exact class/method names against your installed package
version before running (see references/known-issues-watchlist.md for
why "looks right" isn't the same as "verified").
"""

from pathlib import Path

# Package names per public MAF 1.0 docs at verification time.
from agent_framework.declarative import load_workflow_from_yaml
from agent_framework.workflows import InProcessExecutionEngine


def run_vendor_onboarding(submission: dict) -> None:
    workflow_path = Path(__file__).parent / "vendor-onboarding.workflow.yaml"
    workflow = load_workflow_from_yaml(workflow_path)

    engine = InProcessExecutionEngine(workflow)

    # Stream events node-by-node instead of blocking for a final result --
    # see references/executors-and-edges.md on why Events exist.
    for event in engine.run_stream(input=submission):
        print(f"[{event.executor_id}] {event.status}: {event.output}")

