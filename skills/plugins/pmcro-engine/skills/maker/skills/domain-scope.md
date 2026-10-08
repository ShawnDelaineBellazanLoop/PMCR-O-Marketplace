# Maker Domain Skill

The Make role of the PMCR-O loop — passive, composable. Produces a typed `MakerFrame` from a `PlanFrame`.

## Frame Chain

```
Intent Envelope → [Planner] → PlanFrame
PlanFrame       → [Maker]   → MakerFrame
MakerFrame      → [Checker] → CheckerFrame
CheckerFrame    → [Reflector] → ReflectorFrame → next Intent Envelope
```

## MakerFrame Contract

Every frame carries:
- `frame_id` — unique frame identifier
- `trail_id` — trail this cycle belongs to
- `cycle` — cycle number
- `thought_lock` — timestamp lock
- `immutable: true` — frames are immutable once sealed
- `stubs_present` — whether any plan steps were stubbed rather than executed
- `artifacts` — list of `ArtifactRecord` (StepId:Path)

## Key Design Rules

1. **Tool-calling is sequential** — the Maker MUST wait for tool output before emitting the final JSON report. STEP 1 → STEP 2 → STEP 3 (WAIT) → STEP 4. Never complete the task in one pass.
2. **No grammar constraint on the Maker** — the Maker must be free to emit `<tool_call>` tokens. A JSON Schema response format physically bans tool-call tokens at the logit level (grammar poisoning). Use `ChatResponseFormat.Text` for the Maker; keep `ForJsonSchema<T>()` for Planner/Checker/Reflector which never call tools.
3. **Robust JSON extraction** — the Maker's output is a mix of thought-text, tool-calls, and a final JSON block. Extract the **last** fenced ` ```json ... ``` ` block, then fall back to the **last** balanced `{...}` pair — never the first.
4. **Structured output** — the MakerFrame is typed JSON, not prose.

## Guardrails

1. Never plan — only execute the plan.
2. Every artifact is recorded as `StepId:Path`.
3. Frames are immutable once sealed.
4. If a step cannot be executed, mark `stubs_present: true` — never fabricate results.