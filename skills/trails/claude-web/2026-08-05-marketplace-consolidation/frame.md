# Frame — Marketplace Consolidation

**Intent:** Shawn wants everything consolidated into
`W:\PMCR_O\PMCR-O-Marketplace`, deleting older projects as they're
folded in.

**Plan:**
1. Verify `W:\PMCR-O` (root, no `PMCR_O` parent) has nothing unique
   before deleting it.
2. Merge canonical `.pmcro\{laws,constraints,agents,checkpoints}`
   from `W:\PMCR_O\PMCR-O` into this repo as real files (per Shawn's
   explicit choice — reverses the prior pointer-only decision).
3. Delete `W:\PMCR-O` once confirmed disposable.
4. Update `identity.json`, `laws/POINTER.md`, `MEMORY.md` to reflect
   the new state truthfully.

**Make:** Robocopy of the 4 folders (12.9K laws, 8.9K constraints,
5.1K agents, 6.2K checkpoints — all new files, no conflicts).
`W:\PMCR-O` src\ tree checked file-by-file: every project folder
(`ProjectName.Core`, etc.) contained only `bin\`/`obj\`, no `.cs`
source; `.git\` had only an `objects\` folder, no `HEAD`/`refs`/
`config` — confirmed broken/disposable. Deleted via
`Remove-Item -Recurse -Force`.

**Check:** `Test-Path` after delete confirmed removal. Marketplace
`.pmcro\` now has real `laws\colony-laws.md`,
`constraints\earned-constraints.json`, all 5 `agents\*\AGENT.md`,
and 3 `checkpoints\*.md` files on disk.
