# Orchestration Patterns

Built-in shortcuts over the same Executor/Edge primitives -- use these
before reaching for a fully custom graph.

- **Sequential** -- agents run one after another, each seeing the prior
  agent's output. Simplest pattern; equivalent to a straight-line Edge
  chain with no branching.
- **Concurrent** (`AgentWorkflowBuilder.BuildConcurrent`) -- multiple
  agents run in parallel over the same input, e.g. a compliance scanner
  and a duplicate-listing detector evaluating one submission
  simultaneously. Use when agents don't depend on each other's output.
- **Handoff** -- one agent can transfer control to another mid-run based
  on its own assessment (e.g. a general-intake agent handing off to a
  specialist once it classifies the request). Routing decision is made
  by the agent itself, not a pre-declared conditional edge.
- **Group Chat** -- multiple agents participate in a shared conversation
  thread, useful for scenarios needing multi-perspective deliberation
  before a decision.
- **Magentic** -- a manager/orchestrator agent dynamically plans and
  delegates to a pool of specialist agents, adjusting the plan as
  results come in rather than following a fixed graph.

## When To Drop Down To Custom Executors/Edges

The built-in patterns cover common shapes cheaply. Reach for custom
Executors and conditional Edges when the routing logic is business-rule
driven and needs to be explicit and testable -- e.g. "route to manual
fraud review only if risk score > 0.8 AND vendor is unverified." That
condition belongs on an Edge, not inferred by an agent's judgment call,
when the business wants a deterministic, auditable rule rather than an
LLM's routing decision. Custom graphs and built-in patterns compose: a
Concurrent sub-pattern can be one node inside a larger custom graph.
