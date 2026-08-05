---
description: "Manually seal a trail that's stuck (e.g. mid-cycle interruption, MCP instability during writes) after verifying its actual on-disk state. Usage: /orchestrator:seal-trail <trail-uuid>"
---
```
trail_id: <first argument>
repo_path: <the target repo root>
```

Before sealing anything: **EC-VERIFY-FIRST-001 applies.** Read every file
actually present in `.pmcro/trails/<domain>/<trail_id>/` -- do not trust
what a prior turn claimed was written. List the directory, read what's
actually there, then decide.

If the last complete cycle has a `check.jsonl` disposition but no
`disposition.json` at the trail root, write one now per
`../references/trail-schema.md`, using the same TYPE1/TYPE2 rule in
`../references/hil-gating.md` to pick `accept` vs `needs-approval`.

If cycle files are incomplete or contradictory (e.g. a `make.jsonl` with
no matching `plan.jsonl`), do not paper over the gap -- seal
`needs-revision` and note exactly what's missing, so a fresh cycle can
pick it up cleanly rather than building on an uncertain foundation.
