# Agent Harness

A batteries-included runtime layer sitting on top of a standard chat
client, for long, multi-step agent tasks. Exposed as `create_harness_agent`
in Python and `HarnessAgent` in .NET.

## What It Provides Out Of The Box

- **Planning / todo tracking** -- the harness maintains a working plan
  and task list across turns instead of the caller hand-rolling that
  state.
- **Context compaction** -- automatically summarizes/trims context as a
  long-running task approaches the model's context window limit, rather
  than the task failing or the caller writing custom compaction logic.
- **File/memory persistence** -- durable state across the task's
  lifetime, independent of any single turn's context window.

## Why This Matters For PMCR-O

Before the harness existed, this middleware (todo tracking, compaction,
persistence) had to be hand-written per agent. The harness is MAF's
answer to a problem PMCR-O also solves, differently: PMCR-O externalizes
this into sealed trail files read/written explicitly by
Planner/Maker/Checker/Reflector; the harness bakes an equivalent loop
into the runtime itself. Neither is strictly better -- PMCR-O's explicit
trail files are more auditable and inspectable by design (any role, or a
human, can read exactly what happened); the harness trades some of that
transparency for less boilerplate per agent.

## When To Reach For It

If a MAF-native agent needs long-horizon task management and doesn't
need PMCR-O's cross-domain governance (Colony Laws, earned constraints,
HIL gating across TYPE1/TYPE2), the harness is likely less code than
reimplementing the same planning/compaction/persistence loop by hand.
