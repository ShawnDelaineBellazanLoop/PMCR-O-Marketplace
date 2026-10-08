---
name: microsoft-agent-framework
description: "USE FOR: knowledge scope on Microsoft Agent Framework (MAF) 1.0 -- Workflows-as-DAGs (Executors/Edges/Events), built-in orchestration patterns (Sequential, Concurrent, Handoff, Group Chat, Magentic), declarative YAML workflows, Pregel-style checkpointing, the Agent Harness runtime, CodeAct+Hyperlight sandboxed execution, and FIDES information-flow security. Consult whenever a cycle's true_intent concerns building on MAF directly, comparing a PMCR-O mechanism (trails, checkpoints, HIL gates) against MAF's native equivalent, or evaluating adopt-vs-hand-roll for a capability. DO NOT USE FOR: running a PMCR-O cycle -- that's pmcro-loop. DO NOT USE FOR: studying PMCR-O's own evolution against industry standards -- that's framework-evolution, a sibling skill; this skill is MAF reference material only."
metadata:
  pmcro_provides: "microsoft-agent-framework"
  pmcro_requires: ""
compatibility: "Documentation-only knowledge scope, no external runtime dependency. References Microsoft Agent Framework 1.0 APIs current as of mid-2026. Verify against installed package type defs / current docs before treating any version-specific detail as current -- see references/known-issues-watchlist.md."
---

# Microsoft Agent Framework

Knowledge scope for building against Microsoft Agent Framework (MAF) 1.0
directly -- the unified successor to Semantic Kernel + AutoGen for Python
and .NET. This skill exists so a cycle that needs to reason about MAF's
actual primitives (not PMCR-O's) has one place to read from, instead of
re-deriving it from scratch or trusting an untraced summary.

**Provenance note:** most of this skill's content was originally drafted
by a different model (Gemini) in a separate session, then verified
against live web search before being written here. The core
architectural claims (Executors/Edges/Events, Agent Harness,
CodeAct+Hyperlight, FIDES, declarative YAML, Pregel checkpointing)
checked out against Microsoft's own docs. The four specific GitHub issue
numbers that session cited as "known bugs" did **not** get the same
verification -- see `references/known-issues-watchlist.md` before
repeating any of them as fact. This is a live example of why
EC-VERIFY-FIRST-001 applies to imported knowledge, not just to your own
prior turns.

## Why This Matters For PMCR-O

PMCR-O's Plan->Make->Check->Reflect->Orchestrate loop, sealed `.jsonl`
trails, and TYPE1/TYPE2 HIL gating are a hand-rolled analog to things MAF
ships natively: Executors/Edges ~ Planner/Maker/Checker/Reflector role
composition, WorkflowCheckpoint ~ sealed trail frames, FIDES ~ the
TYPE1/TYPE2 tool boundary. Knowing where MAF's native mechanism differs
from PMCR-O's own is what lets `framework-evolution` (a sibling skill)
make an informed adopt-vs-hand-roll call instead of an uninformed one.

## Core Concepts

- **Workflows as DAGs** -- a workflow is an explicit Directed Acyclic
  Graph. Processing units are `Executors` (nodes), connected by `Edges`
  (data paths, optionally with runtime conditions). See
  `references/executors-and-edges.md`.
- **Orchestration patterns** -- Sequential, Concurrent
  (`AgentWorkflowBuilder.BuildConcurrent`), Handoff, Group Chat, and
  Magentic are built-in shortcuts over the same Executor/Edge primitives.
  Custom graphs and built-in patterns compose freely. See
  `references/orchestration-patterns.md`.
- **Declarative workflows** -- graph topology, prompts, and edge
  conditions can live in YAML instead of compiled code
  (`Microsoft.Agents.AI.Workflows.Declarative` for .NET,
  `agent-framework-declarative` for Python). See
  `references/declarative-workflows.md`.
- **Checkpointing** -- a Pregel-style superstep model. Each step
  snapshots state into a `WorkflowCheckpoint`. See
  `references/checkpointing-and-hitl.md`.
- **Agent Harness** -- a batteries-included runtime layer
  (`HarnessAgent` / `create_harness_agent`) providing planning, todo
  tracking, context compaction, and file/memory persistence without
  hand-written middleware. See `references/agent-harness.md`.
- **CodeAct + Hyperlight** -- collapses multi-turn tool-calling into a
  single generated script executed inside a Hyperlight micro-VM sandbox,
  calling tools directly via `call_tool(...)`. See
  `references/codeact-hyperlight.md`.
- **FIDES** -- Python-native information-flow-control security layer
  that gates high-risk tool invocation behind policy and HIL approval.
  See `references/fides-security.md`.

## Worked Example

`assets/vendor-onboarding.workflow.yaml` and
`assets/run_workflow_example.py` show these primitives applied to a
marketplace vendor-onboarding pipeline (tax-doc validation -> conditional
fraud-risk edge -> HIL approval gate -> catalog sync), matching this
repo's own marketplace domain rather than a generic example.
`scripts/validate_workflow_yaml.py` checks the YAML parses and its
declared node/edge references are internally consistent.

## Reference Files

- `references/executors-and-edges.md` -- Executor/Edge/Event primitives,
  typed I/O, why mismatches are caught early
- `references/orchestration-patterns.md` -- Sequential, Concurrent,
  Handoff, Group Chat, Magentic -- built-in pattern vs. custom graph
- `references/declarative-workflows.md` -- YAML workflow shape, how the
  runtime parses and instantiates it
- `references/checkpointing-and-hitl.md` -- WorkflowCheckpoint contents,
  resumability, human-in-the-loop interrupt shape
- `references/agent-harness.md` -- HarnessAgent / create_harness_agent
- `references/codeact-hyperlight.md` -- CodeAct pattern, Hyperlight
  micro-VM sandboxing, `call_tool(...)`
- `references/fides-security.md` -- information-flow-control, policy
  gating, HIL approval triggers
- `references/known-issues-watchlist.md` -- **read before citing any
  specific bug/issue number** -- what's verified vs. imported unverified
