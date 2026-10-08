# Executors, Edges, and Events

## Executor

The fundamental unit of execution in a MAF workflow. Takes a strongly
typed input, runs logic (an LLM call, a DB write, a sub-workflow), and
returns a strongly typed output. Executors are the "nodes" of the graph;
they don't know about the graph they sit in -- routing is the Edge's job,
not the Executor's.

## Edge

Connects one Executor's output to another Executor's input. An edge can
carry an unconditional pass-through or a runtime condition (e.g. route to
a manual-review executor only if a risk score exceeds a threshold).
Conditional edges are how MAF expresses branching without putting
if/else logic inside an Executor's own code.

## Workflow

The compiled graph container, built via `WorkflowBuilder` (or
`AgentWorkflowBuilder` for the higher-level orchestration-pattern
shortcuts -- see `orchestration-patterns.md`). The builder is where
type-compatibility between connected Executors is checked.

## Events

Workflows stream `WorkflowEvent`s as execution progresses node-by-node,
instead of only returning a final result. This is what lets a caller show
live progress, log each step for audit, or drive a UI update per
Executor completion -- comparable to how PMCR-O writes a frame file per
role rather than only a final disposition.

## Why Strong Typing Matters Here

Executor input/output types are checked at the graph-compilation
boundary. If Executor A's declared output type doesn't match Executor
B's declared input type, `WorkflowBuilder` fails to build the graph --
before any agent runs, not at runtime three steps into a live pipeline.
For a system stitching together document-extraction, risk-scoring, and
external-API executors, this converts a class of "LLM returned malformed
JSON three hops downstream" failures into a build-time error.
