---
name: copilotkit
description: "This skill should be used when redesigning the PMCR-O Next.js frontend, implementing CopilotKit or AG-UI changes, improving the shared app shell, or validating frontend agent experiences. It produces a redesign brief and implemented frontend changes."
version: 1.0.0
metadata:
  pmcro_provides: "copilotkit"
  pmcro_requires: "pmcro-framework, dependency-resolver, planner, maker, checker, reflector, playwright-agent"
compatibility: "Next.js frontend under src/frontend; CopilotKit 1.62; AG-UI backend; MAF-compatible PMCR-O skill package."
---

# CopilotKit

Owns the PMCR-O frontend experience: full UI redesign planning, CopilotKit/AG-UI client integration, accessible responsive components, and browser-level validation. Does not own backend agent behavior, PMCR-O loop sequencing, marketplace publishing, or credential management.

## When to use

Trigger for requests to redesign the frontend, refresh the dashboard, improve CopilotKit chat, rework the agent console, connect the UI to AG-UI, or validate the visual agent experience.

## MAF package layout

Keep this skill in the repository convention:

- `SKILL.md` — scope, workflow, metadata, and guardrails.
- `commands/` — explicit invocable procedures.
- `agents/` — optional agent definitions only when a dedicated sub-agent is needed.
- `references/` — detailed architecture and design constraints.
- `scripts/` — deterministic validators or generators; use no package installs unless approved.

## Skill loading

Before implementation, invoke `dependency-resolver` against the PMCR-O catalog and load every relevant plugin skill for the request. At minimum include `pmcro-framework`, `dependency-resolver`, `planner`, `maker`, `checker`, `reflector`, `playwright-agent` or `playwright-mcp`, and any applicable frontend, ASP.NET/AG-UI, testing, security, accessibility, or domain skill. Do not load unrelated skills merely because they exist; resolve by the requested UI scope and dependency metadata.

Do not bypass the catalog or hard-code only the four PMCR-O roles. Record the resolved skill set in the redesign brief and use each loaded skill according to its own scope.

## Workflow

0. Resolve relevant PMCR-O/plugin skills through the catalog and record them in the brief.
1. Inventory `src/frontend`, existing components, `package.json`, `globals.css`, the CopilotKit route, and relevant backend AG-UI comments.
2. Create a redesign brief covering user goals, page map, visual direction, component changes, responsive behavior, accessibility, UI states, and acceptance criteria.
3. Coordinate with `planner`, `maker`, `checker`, and `reflector`; keep this skill focused on frontend execution.
4. Preserve real semantics: PMCR-O roles Planner, Maker, Checker, Reflector; phases Planning, Checking, Reflecting, Sealed; dispositions Accept, Retry, Halt; and agent IDs Orchestrator and Harness.
5. Implement in focused slices: shared shell, page composition, components, responsive rules, accessibility, and CopilotKit surfaces.
6. Keep browser requests on `/api/copilotkit`; keep `AGUI_SERVER_URL` and other service URLs server-only; preserve long-running AG-UI timeout handling.
7. Cover loading, empty, error, disconnected, active-cycle, completed, retry, halted, reduced-motion, keyboard-focus, narrow viewport, and chat-open/closed states.
8. Before testing, verify every new handler variable has a matching state declaration or prop and that the corresponding control is rendered. Validate with the available frontend TypeScript/build checks and Playwright when available. For skill selection, use `.agents/plugins/marketplace.json` as the discovery source and `.pmcro/skills-staging` as the MAF runtime source. Do not modify `.next` or `node_modules` as source files. If Next.js fails on corrupted generated `.next/dev/types`, treat that as an environment/cache issue: preserve the generated files, run a targeted source type check, and report the production compilation result separately.
9. Save briefs and validation summaries under `output/` and report changed files, checks, and unresolved risks.

## Production UI standard

Treat the frontend as an operator product, not a demo page. Keep the assistant drawer optional by default, establish a clear workspace hierarchy, group large catalogs behind search and disclosure, and remove internal implementation language from primary user-facing controls. Prefer real component structure over CSS-only patches. Skill counts and skill-name answers must come from the canonical MAF catalog, never from model memory or recursive command listings.

## Guardrails

- Confirm before destructive edits or replacing the entire design system.
- Do not install packages unless explicitly approved.
- Verify CopilotKit APIs against the installed package version; do not copy examples blindly.
- Reuse existing CSS tokens and typography before introducing new visual primitives.
- Use semantic controls, visible focus states, labels, and reduced-motion support.
- Keep backend behavior and HIL boundaries unchanged unless the user explicitly requests backend work.

Read [project architecture](references/project-architecture.md), [official MAF and CopilotKit guidance](references/official-maf-copilotkit-guidance.md), and [redesign procedure](commands/redesign.md) before implementation.
