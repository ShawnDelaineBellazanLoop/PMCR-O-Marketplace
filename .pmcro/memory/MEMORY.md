# PMCR-O-Marketplace — Memory

Durable facts only. Not a transcript. Sealed trails are the source of
truth for *how* a decision was reached; this file is *what's true now*.
Any tool (Claude.ai, Cline, Codex) should read this before acting, and
append only via a sealed trail's Reflect step.

**CORRECTION (2026-08-05):** an earlier version of this file claimed
"no .pmcro/ existed anywhere in W:\PMCR-O or this marketplace repo
before 2026-08-05." That was wrong — never actually verified against
the canonical repo. `W:\PMCR-O\.pmcro\` is the real, months-deep
canonical store (laws/, constraints/, agents/, dozens of sealed
trails back to 2026-05-29, an earned-constraints ledger, checkpoints
from 2026-07-21/23). This marketplace `.pmcro\` is a second, newer,
much smaller store that grew independently this session. **Two
stores now exist and are diverging.** Shawn has not yet said which
one wins — do not silently pick one. Until he decides, tools working
in this marketplace repo should treat this file as scoped ONLY to
marketplace-repo facts, and check `W:\PMCR-O\.pmcro\memory\` (if one
exists there) for anything cross-repo.

## Current state (as of 2026-08-05)

- Marketplace repo has 3 pmcro-* plugins (engine, csuite, specialty)
  and 17 dotnet-* plugins, listed in `.claude-plugin/marketplace.json`.
- Every plugin has both `.claude-plugin/plugin.json` and
  `.codex-plugin/plugin.json` — parity achieved 2026-08-05, sealed
  trail `trails/claude-web/2026-08-05-marketplace-plugin-completion/`.
- `commands/` sits at plugin root for csuite and specialty (one .md
  per user-facing slash command). **RESOLVED:** engine has no
  commands/ by design — its 5 skills are internal cycle machinery
  dispatched only by orchestrator, never user-invoked directly. See
  sealed trail `2026-08-05-engine-commands-resolution/`. Marketplace
  plugin structure is now considered COMPLETE.
- `.agents/skills`, `.github/skills`, `.github/agents` are
  intentionally empty placeholders (dotnet/skills upstream
  convention) — do NOT populate; real skills live under
  `plugins/<name>/skills/`.

## Known open items (cross-repo, unresolved)

- Repo-wide sweep for silently-truncated 0-byte files, prompted by
  the `PmcroLoop.cs` incident — NOT run against this marketplace repo
  yet (it WAS run and closed against the main W:\PMCR-O repo, per that
  repo's own sealed trail `2026-08-05-skills-catalog-microservice`,
  which also found/fixed the 0-byte PmcroLoop.cs itself; the *sweep
  for other files* is still an open follow-up even in the main repo).
- `domain-specialist/SKILL.md` content discrepancy (on-disk content
  doesn't match expected property-preservation domain content) — open
  in the main repo; not yet checked against this marketplace repo's copy.
- **Store consolidation: REVERSED (2026-08-05).** Shawn decided to
  consolidate everything into this repo instead. `W:\PMCR-O` (root,
  no `PMCR_O` parent — confirmed to be nothing but stale build
  artifacts and a broken `.git`) has been deleted. `laws\`,
  `constraints\`, `agents\`, `checkpoints\` were merged from
  `W:\PMCR_O\PMCR-O\.pmcro\` into this repo's `.pmcro\` as real
  files — see `laws/POINTER.md` (now a merge record, not a live
  pointer) and trail `2026-08-05-marketplace-consolidation`.
  `W:\PMCR_O\PMCR-O` (the canonical repo) itself has NOT been fully
  deleted yet.
- **22 legacy skills folded in (2026-08-05).** Canonical `skills\`
  (38 folders) vs this repo's `plugins\*\skills\` (17 folders) left
  22 skills never migrated (checker-agent, filesystem-agent,
  terminal-agent, playwright-agent, pmcro-framework, and 17 others).
  Copied into a new `plugins/pmcro-legacy/` plugin, registered in
  `marketplace.json`. **Retroactively re-sealed 2026-08-05** as a
  real GUID-folder trail — the original claude-web trails for this
  (`2026-08-05-canonical-skills-and-pmcro-fold-in`,
  `2026-08-05-marketplace-consolidation`) were narrative seal.json
  files, NOT real sealed trails (no GUID folder, no phase JSONL) —
  see `EC-2026-08-05-001` and trail `trails/cto/d706d932-bd69-4146-8b84-52eccc5598b0/`.
- **src\ and mcp\ moved in (2026-08-05).** The real .NET runtime
  application (`ProjectName.AppHost`, `.Core`, `.OrchestratorApi`,
  `.ServiceDefaults`, `Services`, `frontend` under `src\`; the 3 MCP
  servers — Filesystem, Playwright, Terminal — under `mcp\`) moved
  from canonical `W:\PMCR_O\PMCR-O` into this repo's root as `src\`
  and `mcp\`. **This repo is no longer skills-only** — it now holds
  both the plugin marketplace AND the runtime app. Verified present
  in both locations directly (moved, not copied) before/after the
  move via `list_directory`.
- **Still remaining in canonical, NOT yet moved:** `catalog\`,
  `docs\`, `skills\` (the pre-legacy-fold-in original, since only the
  22-skill *gap* was copied, not the full 38). These need
  reconciliation against this repo's own `docs\`/`plugins\` before
  canonical can be considered safe to delete.

## Trail index

- `2026-08-05-marketplace-plugin-completion` — sealed, ACCEPT
- `2026-08-05-engine-commands-resolution` — sealed, ACCEPT
- `2026-08-05-pmcro-scaffold-scoping` — sealed, ACCEPT — **SUPERSEDED
  by `2026-08-05-marketplace-consolidation` below.** Original decision
  was repo-local-only .pmcro; Shawn reversed this same day.
- `2026-08-05-marketplace-consolidation` — sealed, ACCEPT — merged
  laws/constraints/agents/checkpoints from canonical into this repo
  as real files; deleted the disposable `W:\PMCR-O` root dupe.
  **NOTE:** this trail is itself a narrative seal.json, not a real
  sealed trail — flagged by `EC-2026-08-05-001`, not yet re-sealed.
- `2026-08-05-canonical-skills-and-pmcro-fold-in` — narrative
  seal.json only (not a real sealed trail) — retroactively verified
  and re-sealed as `trails/cto/d706d932-bd69-4146-8b84-52eccc5598b0/`
  — sealed, ACCEPT.
- `trails/cto/d706d932-bd69-4146-8b84-52eccc5598b0/` — sealed, ACCEPT
  — real GUID-folder trail retroactively verifying the pmcro-legacy
  fold-in; created `EC-2026-08-05-001`.
- `trails/cto/b29533b0-a4c3-439f-9ed6-e4e8b5eb9596/` — sealed, ACCEPT
  — moved `src\` and `mcp\` from canonical `W:\PMCR_O\PMCR-O` into
  this repo's root; canonical still holds catalog/docs/skills,
  unmoved.
