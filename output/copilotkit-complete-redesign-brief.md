# CopilotKit Frontend — Complete Production Redesign Brief

## Problem found

The existing UI presents a prototype-like command page with a large empty hero, a crowded skill catalog, a wide assistant drawer, and competing visual systems. The chat transcript also exposes a data-integrity issue: the UI reports 135 filesystem-derived entries while the assistant reports 118 MAF-indexed skills and then lists recursive command names as skills.

## Product direction

Build a calm, operator-first command center. The task is primary; skills and routing are supporting context; live PMCR-O evidence is visible after a run; the assistant is available without taking over the screen.

## Implemented structure

- Persistent rail becomes responsive navigation: desktop rail, compact tablet rail, bottom mobile bar.
- Main console becomes a workspace header, governed-run composer, routing context, skill context, live phase evidence, round table, and trail player.
- Domain routing becomes a compact native select instead of ten competing pills.
- Skill context remains searchable and grouped, with collapsed plugin sections and selected chips.
- CopilotKit drawer width is constrained relative to the available workspace.
- Orchestrator and Harness receive a deterministic `get_skill_catalog` tool.

## Catalog contract

The authoritative catalog is unique `SKILL.md` names from `.pmcro/skills-staging`, populated from `.agents/plugins/marketplace.json` by `MarketplaceSkillsMaterializer`. Commands, tool names, and generated model text are not catalog entries.

## Acceptance criteria

- “How many skills are there?” calls `get_skill_catalog` and returns its count.
- “What are they?” returns canonical skill names, grouped or paginated, without recursive command variants.
- UI and chat use the same unique-name definition.
- Frontend remains behind `/api/copilotkit`; backend remains at `/agui`.
- Selected skill context is explicit and MAF resolves actual loading.
- Responsive layout works at desktop, tablet, and mobile widths.
- Frontend source type check and .NET solution build pass.

## Validation result

- Frontend targeted source TypeScript check: passed.
- Full PMCR-O .NET solution build: passed with 0 warnings and 0 errors.
- Backend tail corruption in `Program.cs` was repaired before the successful build.

## Operational handoff

Restart the AppHost/Orchestrator service before testing the new `get_skill_catalog` tool. Start a new CopilotKit conversation after restart; existing transcripts remain historical and will not be corrected retroactively.
