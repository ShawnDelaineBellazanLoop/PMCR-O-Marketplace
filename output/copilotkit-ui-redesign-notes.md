# CopilotKit UI redesign notes

## Product direction

The frontend is an operator workspace, not a demo page. The primary job is to submit a task with optional domain and MAF skill context, then monitor verified PMCR-O work.

## Structural decisions

- Keep the CopilotKit assistant drawer optional by default.
- Keep the main workspace visible when chat is closed.
- Use `.agents/plugins/marketplace.json` for skill discovery.
- Use declared `SKILL.md` names for selected-skill IDs.
- Keep `.pmcro/skills-staging` as the runtime staging path.
- Group large skill catalogs behind search and disclosure.
- Keep internal implementation details out of primary user-facing copy.
- Preserve real PMCR-O phase, disposition, trail, and HIL semantics.

## Regression prevention

Before any UI test, verify new component state, props, handlers, and rendered controls together. Do not use CSS-only redesign patches when markup hierarchy is the source of the problem.
