# Checkpointing and Human-in-the-Loop

## Pregel Superstep Model

MAF workflow execution proceeds in supersteps, borrowing the Pregel
bulk-synchronous-parallel model: each superstep, every ready Executor
runs, and after the step completes the runtime serializes a
`WorkflowCheckpoint`. A checkpoint captures (at minimum):

- in-flight messages between Executors
- per-Executor local state
- iteration/loop counters
- pending human-in-the-loop request state

If execution is interrupted -- a crash, a timeout, a process restart --
reloading the last checkpoint resumes from that exact superstep rather
than restarting the workflow from scratch.

## Comparison To PMCR-O Trails

This is the closest MAF-native analog to a PMCR-O sealed trail: a
`WorkflowCheckpoint` is to a MAF workflow what a cycle's frame + `.jsonl`
files are to a PMCR-O trail -- both exist so a long-running process can
resume from evidence on disk instead of re-deriving what already
happened. The difference is granularity: MAF checkpoints at every
superstep automatically; PMCR-O currently checkpoints at cycle
boundaries (Plan/Make/Check/Reflect), not sub-step.

## Human-in-the-Loop (HITL)

An Executor can raise a request that pauses the workflow pending human
input (approve/reject/modify) before downstream Edges fire. This is
MAF's native equivalent of PMCR-O's TYPE1/TYPE2 tool boundary --
TYPE1 (world-changing) actions get HIL-gated, TYPE2 (read-only) don't.
When building on MAF directly, prefer wiring irreversible or
cross-domain actions (the same class of action PMCR-O's `hil-gating.md`
reference describes) behind a HITL-gated Executor rather than trusting
an agent's own judgment to pause.
