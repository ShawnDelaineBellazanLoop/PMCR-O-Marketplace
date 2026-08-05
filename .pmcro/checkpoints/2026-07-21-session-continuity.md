# Checkpoint — 2026-07-21

**Read this file first in a new session.** It's a snapshot of decisions and
open items from a live design session, so a fresh session can resume from
here instead of re-deriving context from chat history or the privacy-export
dump (`conversations.json`/`memories.json`).

## Decided this session

- **Repo structure: hub-and-spoke, not monorepo.** `W:\PMCR-O` is the hub
  (`src/` runtime, its own `.pmcro/`, `repos/registry.json` as the index).
  `.agents/` is the portable skill library, decoupled from any one hub's
  lifecycle. `repos/<name>` entries are real independent repos, each with
  their own `.pmcro/`, registered in the hub's registry. This is
  **SYNTHESIZED, not yet confirmed** — treat as a working recommendation
  until explicitly signed off.
- **`TrailRef` primitive proposed** for cross-repo/cross-domain trail
  references: `{from_trail, to_trail, to_repo, to_domain, relationship,
  version_at_reference}`. `version_at_reference` is the load-bearing field —
  it's what makes a stale cross-repo reference detectable instead of silently
  wrong, borrowed from the `if_version` optimistic-concurrency pattern in
  Claude's own memory API. Relationship types: `derived-from`, `supersedes`,
  `consulted` (maps to Pattern B), `disputes`. **Not yet checked against
  whatever CopilotKit/MAF Harness UI already does** — flagged
  UNVERIFIED last cycle.
- **EC-003 (connector root cause):** Filesystem MCP is the connector that's
  actually dead; Desktop Commander is a separate, working connector with
  unrestricted (`allowedDirectories: []`) access to this machine. Don't
  retry Filesystem MCP cold in a future session — try Desktop Commander
  first.
- **This `.pmcro/` scaffold itself** — `sessions/ trails/ frames/ memory/
  checkpoints/ registry/ evaluations/ cache/` — was a previously-decided
  architecture item that had never actually been built. It's built now.

## Still open / unconfirmed

- `repos/registry.json` is live but empty (`"repos": []`) — no child repos
  registered yet.
- `skills/*/` folders (codeact-agent, filesystem-agent, playwright-agent,
  terminal-agent) currently contain **only** `SKILL.md` — missing the
  agentskills.io `scripts/ references/ assets/ evals/` subfolders. Not
  touched this session; flagged so it isn't mistaken for compliant.
- Whether `TrailRef` should live as a new frame type in `frames/` or as
  metadata on existing frame types — not decided.
- Cycle-1 baton from the prior transcript (diff scaffolded
  `PmcroLoop.cs`/gate-wiring against the real `src/` tree) — not yet done
  this session; `src/` layout confirmed as standard per-service `.csproj`
  (`services/ProjectName.<Role>Service/`, `ProjectName.Core`,
  `ProjectName.AppHost`) but no line-by-line diff run.

## What NOT to re-litigate

- Pattern A / Pattern B / O-Mode distinctions — settled in
  `references/cascade-and-omode.md`, don't re-derive from scratch.
- EC-001 (layer boundary) and EC-002 (Reflector recursion) — settled in
  `references/constraint-ledger.md`.
