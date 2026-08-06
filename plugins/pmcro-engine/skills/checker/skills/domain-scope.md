# Checker Domain Skill

The Check role of the PMCR-O loop — passive, composable. Produces a typed `CheckerFrame` from a `MakerFrame`.

## Frame Chain

```
Intent Envelope → [Planner] → PlanFrame
PlanFrame       → [Maker]   → MakerFrame
MakerFrame      → [Checker] → CheckerFrame
CheckerFrame    → [Reflector] → ReflectorFrame → next Intent Envelope
```

## Six-Dimension Scoring Contract

| Dimension | Weight |
|---|---|
| Intent Coverage | 0.25 |
| Plan Fidelity | 0.25 |
| Completeness | 0.20 |
| Code Quality | 0.10 |
| Test Coverage | 0.15 |
| Trail Compliance | 0.05 |

This is the canonical evaluation contract. The Planner writes `acceptance_criteria` that map directly to these dimensions — which closes the loop between planning and checking.

## CheckerFrame Contract

Every frame carries:
- `frame_id` — unique frame identifier
- `trail_id` — trail this cycle belongs to
- `cycle` — cycle number
- `thought_lock` — timestamp lock
- `immutable: true` — frames are immutable once sealed
- Dimension scores: `intent_coverage`, `plan_fidelity`, `completeness`, `code_quality`, `test_coverage`, `trail_compliance`

## Key Design Rules

1. **Evidence-based verdicts** — a FAIL verdict must be backed by rationale and specific scored dimensions.
2. **`GapRecord` typed model** — known gaps are a typed list of `GapRecord` objects, not `List<string>`. Qwen 2.5 emits structured gap objects; untyped strings cause `JsonException` during deserialization.
3. **Verify, don't assume** — Checker verifies Maker's claimed StepResults against real on-disk/environment evidence before scoring.
4. **Structured output** — the CheckerFrame is typed JSON, not prose.

## Guardrails

1. Never plan or execute — only check.
2. Every dimension scored explicitly; no implicit pass.
3. Frames are immutable once sealed.