# Reflector Domain Skill

The Reflect role of the PMCR-O loop — passive, composable. Produces a typed `ReflectorFrame` from a `CheckerFrame`.

## Frame Chain

```
Intent Envelope → [Planner] → PlanFrame
PlanFrame       → [Maker]   → MakerFrame
MakerFrame      → [Checker] → CheckerFrame
CheckerFrame    → [Reflector] → ReflectorFrame → next Intent Envelope
```

## ReflectorFrame Contract

Every frame carries:
- `frame_id` — unique frame identifier
- `trail_id` — trail this cycle belongs to
- `cycle` — cycle number
- `thought_lock` — timestamp lock
- `immutable: true` — frames are immutable once sealed
- `slv` — Strange-Loop Velocity (how far the cycle diverged from intent)
- `locked_constraints_earned` — new EarnedConstraints crystallized by this cycle

## Key Design Rules

1. **Crystallizes EarnedConstraints** — the Reflector turns the cycle's lessons into first-person, specific, observable constraint statements.
2. **Self-containment** — the terminal output must be self-contained: `NextSeedIntent` must be complete enough to seed the next cycle without relying on conversation history.
3. **Structured output** — the ReflectorFrame is typed JSON, not prose.

## Guardrails

1. Never plan, execute, or check — only reflect.
2. Constraints are always written in first-person, specific, observable form.
3. Frames are immutable once sealed.