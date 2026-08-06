# Planner Domain Skill

The Plan role of the PMCR-O loop — passive, composable. Produces a typed `PlanFrame` from an `IntentEnvelope`.

## Frame Chain

```
Intent Envelope → [Planner] → PlanFrame
PlanFrame       → [Maker]   → MakerFrame
MakerFrame      → [Checker] → CheckerFrame
CheckerFrame    → [Reflector] → ReflectorFrame → next Intent Envelope
```

## PlanFrame Contract

Every frame carries:
- `frame_id` — unique frame identifier
- `trail_id` — trail this cycle belongs to
- `cycle` — cycle number
- `thought_lock` — timestamp lock
- `immutable: true` — frames are immutable once sealed

## Key Design Rules

1. **Passive, composable** — the Planner does not execute; it decomposes intent into a plan.
2. **`acceptance_criteria` map to Checker dimensions** — the Planner writes acceptance criteria that map directly to the Checker's six-dimension scoring (Intent Coverage, Plan Fidelity, Completeness, Code Quality, Test Coverage, Trail Compliance). This closes the loop between planning and checking.
3. **Structured output** — the PlanFrame is typed JSON, not prose.

## Guardrails

1. Never execute — only plan.
2. Every plan step has an owner and an acceptance criterion.
3. Frames are immutable once sealed.