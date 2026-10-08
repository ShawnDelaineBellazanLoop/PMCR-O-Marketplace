# PMCR-O Frame — 2026-08-05-marketplace-plugin-completion

subjectAgent: claude-web
tool: Claude.ai (Desktop Commander MCP)
domain scope: cto (plugin/marketplace architecture)
O-Mode: Full PMCR-O cycle (auditable trail warranted — cross-tool
loadability of published plugins, correctness matters)

## Plan (I am the Planner, cycle 1)

True intent: make the three pmcro-* plugins (engine, csuite, specialty)
loadable by third-party LLM tools (Codex CLI and others), matching the
dotnet-* plugin convention already proven in this marketplace.

Established facts:
- dotnet-* plugins each carry `.claude-plugin/plugin.json` AND
  `.codex-plugin/plugin.json`.
- pmcro-* plugins had `.claude-plugin/plugin.json` only — this is the gap.
- Claude Code's own plugin-structure docs confirm `commands/` must sit at
  plugin root (auto-discovered), not nested under `skills/<name>/commands/`.

Steps:
1. Create `.codex-plugin/plugin.json` for pmcro-engine, pmcro-csuite,
   pmcro-specialty, mirroring the minimal dotnet-* shape
   (name/version/description/skills glob).
2. Verify commands/ placement at plugin root for csuite and specialty.
3. Leave `.agents/skills` empty per dotnet/skills upstream convention —
   confirmed this is a placeholder, not a required population target.

## Make (I am the Maker, cycle 1)

Executed steps 1–2 from the plan across two sessions (context-interrupted
once, resumed cleanly via filesystem state rather than transcript memory):
- Wrote `.codex-plugin/plugin.json` for all three pmcro-* plugins.
- Confirmed `commands/` already sits at plugin root for pmcro-csuite and
  pmcro-specialty (present from earlier work this session).
- pmcro-engine has no `commands/` dir — this was not asked for and is not
  assumed; flagged below as SYNTHESIZED, not silently added.

## Check (I am the Checker, cycle 1)

Per-claim:
- "All three .codex-plugin/plugin.json files exist and are well-formed" —
  VERIFIED (read back all three, valid JSON, correct name/skills glob).
- "csuite and specialty have root-level commands/" — VERIFIED (listed).
- "pmcro-engine needs a commands/ dir too" — UNVERIFIED / not in scope —
  engine's SKILL.md set (planner/maker/checker/reflector/orchestrator) may
  be command-less by design (they're invoked as role-skills, not slash
  commands). Flag for Shawn to confirm rather than assume.
- ".agents/skills being empty is correct, not a gap" — SYNTHESIZED from
  dotnet/skills upstream pattern observed elsewhere in repo; not
  independently confirmed against agentskills.io spec text.

Disposition: ACCEPT for the stated plan scope (Codex-loadability gap
closed). One open question surfaced for Shawn, not silently resolved.

## Reflect (I am the Reflector, cycle 1)

Recurring pattern worth naming: this is the second time a structural gap
(missing commands/ or missing .codex-plugin) was found by diffing pmcro-*
plugins against the proven dotnet-* plugins in the same repo, rather than
against external spec docs. Candidate constraint: when this repo already
contains a working reference implementation of a convention, diff against
it first — it's a stronger source of truth here than re-deriving from
upstream docs alone.

Recommend: next cycle should confirm the engine commands/ question with
Shawn directly (Elicitation O-Mode), not assume either way.
