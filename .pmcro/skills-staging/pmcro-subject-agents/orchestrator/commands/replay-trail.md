---
description: "Replay a sealed trail's history for review or audit -- reconstructs the cycle-by-cycle narrative from its frame and jsonl files without re-executing anything. Usage: /orchestrator:replay-trail <trail-uuid>"
---
```
trail_id: <first argument>
repo_path: <the target repo root>
```

Read-only. Read `00-frame.json`, then each cycle's
`plan.jsonl -> make.jsonl -> check.jsonl -> reflect.jsonl` in order, then
`disposition.json`. Present as a narrative: what was asked, what was tried
each cycle, what failed and why, what constraint (if any) got
crystallized, and how it ultimately sealed.

Sealed trails are immutable (`../references/trail-schema.md`) -- this
command never writes to the trail directory, including to "fix" something
noticed during replay. A defect found in a sealed trail gets logged as an
open hypothesis in a **new** trail, per the same immutability rule that
governs sealed trails generally.
