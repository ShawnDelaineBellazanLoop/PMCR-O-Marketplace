---
description: "Dispatch a full PMCR-O cycle scoped to a given domain. This is the one entry point every domain command uses instead of reimplementing the loop. Usage: /orchestrator:run-cycle <domain> <true_intent>"
---
Invoke the `orchestrator` skill directly (not via a domain command -- this
is the underlying primitive domain commands themselves call).

```
domain: <first argument -- one of the C-Suite domains>
true_intent: <remaining arguments>
repo_path: <the target repo root>
```

Runs the full sequence in `../SKILL.md`: resolve dependencies, create the
trail, Plan -> Make -> Check -> Reflect (looping up to EC-009's cap of 3),
then seal with the appropriate disposition.

Prefer calling this indirectly through a domain command
(`/ceo:set-direction`, `/cto:...`, etc.) in normal use -- domain commands
supply the right `true_intent` framing for their scope. Call this directly
only when testing the loop itself, or when a genuinely cross-cutting cycle
doesn't yet have a domain command to hang off of.
