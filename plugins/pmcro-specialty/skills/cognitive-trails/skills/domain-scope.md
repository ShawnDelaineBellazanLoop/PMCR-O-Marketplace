# Cognitive Trails Domain Skill

Manages the cognitive trail substrate — the persistent record of every PMCR-O cycle's frames, decisions, and earned constraints.

## Role

The Cognitive Trails skill owns the trail lifecycle: creation, frame append, sealing, and reconstruction. Every frame carries `frame_id`, `trail_id`, `cycle`, `thought_lock`, and `immutable: true` so the full trail is reconstructable from just the artifact folder, with no reliance on conversation history.

## Key Design Rules

1. **Trail reconstruction** — the full trail is reconstructable from just the artifact folder, with no reliance on conversation history.
2. **Immutable frames** — frames are immutable once sealed.
3. **Self-containment** — `NextSeedIntent` must be complete enough to seed the next cycle without relying on conversation history.

## Guardrails

1. Every trail item names its `trail_id`, `cycle`, and `frame_id`.
2. Frames are immutable once sealed.
3. "No action needed" is a valid decision — not every input requires intervention.