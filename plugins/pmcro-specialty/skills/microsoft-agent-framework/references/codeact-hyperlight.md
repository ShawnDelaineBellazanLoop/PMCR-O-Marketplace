# CodeAct + Hyperlight Sandboxing

## The Problem It Solves

Standard tool-calling is a multi-turn round trip: the model calls one
tool, waits for the result, gets it back in context, decides the next
call, repeats. For a task needing many sequential tool calls, that's
many LLM turns, each adding latency and token overhead.

## CodeAct Pattern

Instead of one tool call per turn, the model generates a single script
(Python or JS) that orchestrates the whole multi-step sequence itself,
calling tools directly via `call_tool(...)` from inside the script. The
model reasons once, writes the plan as code, and the code executes the
full sequence without further round trips to the model for each
intermediate step.

## Hyperlight Micro-VM Sandbox

That generated script runs inside a Hyperlight micro-VM
(`agent-framework-hyperlight` package) rather than directly on the host
-- a lightweight, fast-starting VM boundary so a model-generated script
executing arbitrary logic can't touch the host process or filesystem
outside its granted tool surface.

## Trade-off To Note

This pattern trades per-step visibility for throughput: with classic
tool-calling, each call and result is a discrete, inspectable turn (and
a natural HIL checkpoint). With CodeAct, an entire multi-step plan
executes as one script -- good for latency and token cost, but any
approval gate now needs to sit around the whole script's execution (or
around individual `call_tool` invocations inside the sandbox), not
between turns the way it would in classic tool-calling. Factor that into
where HIL gates get placed if adopting CodeAct for anything touching a
TYPE1-equivalent action.
