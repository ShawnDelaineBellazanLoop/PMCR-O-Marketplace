# Knowledge Synthesis Reference — Domain Specialist

Distillation methodology and pattern recognition for session exports.

## Distillation Output Format

Every distilled session produces:

```json
{
  "session_id": "export identifier",
  "date_range": "when the conversation occurred",
  "participants": ["roles or identifiers"],
  "decisions_made": [
    {"decision": "what was decided", "context": "why", "confidence": "explicit|implicit"}
  ],
  "facts_established": [
    {"fact": "what was confirmed", "source": "where in the session"}
  ],
  "open_questions": [
    {"question": "what's unresolved", "owner": "who should answer"}
  ],
  "action_items": [
    {"action": "what needs doing", "owner": "who", "deadline": "when or null"}
  ],
  "knowledge_tags": ["reusable categories"]
}
```

## Pattern Extraction Rules

A pattern requires:

1. **Recurrence**: observed in 2+ distinct sessions
2. **Specificity**: not "communication issues" but "API contract changes not communicated to client teams"
3. **Evidence**: cite the specific sessions and approximate position

Pattern output:

```json
{
  "pattern": "description of the recurring phenomenon",
  "frequency": "observed in N of M sessions analyzed",
  "sessions": ["id1", "id2"],
  "impact": "what this pattern causes",
  "recommendation": "suggested intervention"
}
```

## Memory Hydration

When writing to Colony memory:

1. Follow the target system's exact format (markdown, JSON, YAML)
2. Include source attribution: session ID + approximate position
3. Separate fact from interpretation: "Agent reported X" vs "X appears to be true"
4. Do not overwrite existing memory unless explicitly instructed