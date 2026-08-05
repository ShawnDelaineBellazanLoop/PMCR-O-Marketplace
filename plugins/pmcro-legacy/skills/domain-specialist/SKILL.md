---
name: domain-specialist
description: "Domain Specialist skill — chat distillation, pattern extraction, memory hydration, and LLM session export processing."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=domain-specialist."
pattern_d: opt-in
---

# Domain Specialist Skill

Knowledge & Synthesis specialist. Distills raw LLM chat/session exports into
structured knowledge, extracts recurring patterns across sessions, and hydrates
Colony memory/context from accumulated conversation history.

## Owns
- Chat/session distillation into structured knowledge
- Pattern extraction across multiple sessions
- Memory hydration from session exports
- LLM session export processing

## Does Not Own
- Company-wide SOP design (`coo` — this domain synthesizes within COO's framework)
- Financial reporting (`cfo`)
- Contract legal terms (`clo`)

## Reports To
`coo`

## Scripts

| Script | Purpose |
|---|---|
| `session_distiller.py` | Extract structured knowledge, decisions, and action items from chat exports |
| `pattern_extractor.py` | Find recurring themes, conflicts, and resolutions across multiple sessions |

## References
- `references/knowledge-synthesis.md` — Distillation methodology and pattern recognition

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/domain-specialist/<uuid>/`. See
`../orchestrator-agent/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Distilled knowledge separates: decisions made, facts established, open questions.
2. Patterns require evidence from 2+ sessions — a single occurrence is not a pattern.
3. Memory hydration entries follow the target system's format exactly.
4. Source attribution: every extracted item cites the session ID and approximate position.
