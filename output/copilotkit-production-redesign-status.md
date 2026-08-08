# CopilotKit production redesign status

## Completed

- Replaced raw plugin-folder scanning with `.agents/plugins/marketplace.json` discovery.
- Resolved each registered plugin source using the same repository-relative path as MAF.
- Matched selected skill IDs to declared `SKILL.md` names used by the MAF loader.
- Replaced the 135-item flat list with collapsed plugin groups, search, selected chips, and responsive controls.
- Added production command-workbench styling and explicit `.pmcro/skills-staging` status.
- Added backend parsing for `[skills: skill-a, skill-b]` and preserved the explicit names in cycle intent for the native `AgentSkillsProvider`.
- Preserved `/api/copilotkit` as the browser boundary and `/agui` as the backend AG-UI boundary.

## Validation

- Targeted TypeScript check for `skills.ts`, `SkillSelector.tsx`, `ConsoleView.tsx`, and `page.tsx`: passed.
- Full Next.js type phase remains blocked by the pre-existing corrupted `.next/dev/types/app/api/copilotkit/route.ts` generated file.

## Runtime source of truth

- Discovery registry: `.agents/plugins/marketplace.json`
- Registered plugin source folders: `plugins/*/skills/*/SKILL.md`
- MAF materialized runtime: `.pmcro/skills-staging`
- Native runtime loader: `AgentSkillsProvider`

Selected skills should only be described as active after the MAF provider confirms that they were loaded.
