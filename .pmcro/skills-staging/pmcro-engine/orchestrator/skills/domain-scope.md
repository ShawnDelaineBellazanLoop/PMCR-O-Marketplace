# Orchestrator Domain Skill

The Orchestrate role of the PMCR-O loop — owns sequencing, loop-vs-seal, and EC-009 MaxLoops enforcement.

## Role

The Orchestrator is the **only entity that holds the full intent**. It decides which tools to call in sequence to satisfy that intent. Skills are **siblings, not a hierarchy** — neither knows about the other. The Orchestrator is the hub:

```
git_status → returns modified paths
     ↓
Orchestrator sees paths in src/ and skills/
     ↓
Planner decomposes: "to reason about these changes, I need project structure"
     ↓
Maker invokes: list_project(root_path, sub_path="src")
     ↓
Checker validates the data is sufficient
     ↓
Now the loop has both: what changed (git) + what exists (filesystem)
```

## Frame Chain Ownership

```
Intent Envelope → [Planner] → PlanFrame
PlanFrame       → [Maker]   → MakerFrame
MakerFrame      → [Checker] → CheckerFrame
CheckerFrame    → [Reflector] → ReflectorFrame → next Intent Envelope
```

The Orchestrator sequences each frame and passes it to the next phase. It enforces:
- **Loop-vs-seal** — decides whether the cycle loops again or seals the trail
- **EC-009 MaxLoops** — enforces the maximum loop count (default 5)
- **Dispatch decisions** — TYPE 2 tools (physical actuators) are excluded from `dispatch_decisions`; only cognitive routing is recorded

## Key Design Rules

1. **The Orchestrator owns intent, not the skills.** Skills never decide what to call next.
2. **Sequential workflow enforcement** — the Maker's prompt forces it to wait for tool output before emitting the final JSON summary.
3. **Type separation** — TYPE 1 tools are cognitive (in-process); TYPE 2 tools are physical (MCP actuators). TYPE 2 tools never enter `dispatch_decisions`.
4. **MaxLoops gate** — when `loop_count >= 5`, the cycle terminates with an `ESCALATED` result rather than looping again.

## Guardrails

1. Never let skills call each other directly — always route through the Orchestrator.
2. Enforce loop-vs-seal on every cycle boundary.
3. Frames are immutable once sealed.