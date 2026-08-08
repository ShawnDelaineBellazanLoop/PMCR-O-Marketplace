# CopilotKit Frontend Skill Console Redesign

## Goal

Make the CopilotKit frontend a usable skill-enabled agent console. Users can discover plugin skills, select one or more skills, and submit a task to the PMCR-O Orchestrator with an explicit skill-selection prefix.

## Scope

- Add a server-side reader for plugin skill metadata.
- Add a searchable, keyboard-accessible multi-select skill picker to the Console.
- Preserve the existing CopilotKit `/api/copilotkit` bridge and AG-UI backend boundary.
- Preserve Orchestrator/Harness chat modes and PMCR-O phase semantics.
- Keep the backend catalog as the execution source of truth.

## UI direction

Use the existing dark neutral token system, compact control density, visible selected states, and semantic form controls. The picker should feel like an operator command surface rather than a decorative marketplace.

## Request contract

When skills are selected, the Console sends `[skills: skill-a, skill-b]` before the user request. The Orchestrator remains responsible for resolving dependencies, loading skill content, enforcing permissions, and reporting what was actually loaded.

## Acceptance criteria

- Skill metadata is loaded server-side from plugin `skills/*/SKILL.md` files.
- The picker supports search, plugin grouping, select/deselect, and clear all.
- Selected skills are visible before submission.
- Unselected requests remain backward-compatible.
- No server-only filesystem paths or skill contents are shipped to the browser.
- TypeScript and production build checks pass.
- Empty, loading, no-results, keyboard-focus, narrow viewport, and disabled-submit states are usable.

## Resolved skills

This redesign uses CopilotKit, PMCR-O governance/dependency rules, planner, maker, checker, reflector, and frontend validation guidance. Browser verification should use Playwright when available.

## Implementation status

Implemented the frontend portion:

- `app/lib/skills.ts` reads plugin skill metadata server-side.
- `app/components/SkillSelector.tsx` provides search, multi-select, clear-all, selected styling, keyboard-native checkboxes, and narrow-screen layout.
- `app/page.tsx` loads the catalog in the Server Component and passes summaries to the client.
- `app/components/ConsoleView.tsx` includes selected IDs in a `[skills: ...]` request prefix.
- `app/globals.css` adds the skill-console visual system and responsive states.
- `tsconfig.json` was reviewed for generated type inclusion.

## Validation

- Production Webpack compilation: passed.
- Targeted TypeScript check for the new skill reader and selector: passed.
- Full Next.js type phase: blocked by a pre-existing corrupted generated file at `.next/dev/types/app/api/copilotkit/route.ts`; no generated cache files were deleted.

## Backend follow-up

The Orchestrator currently documents and parses `[domain: ...]`, but no deterministic selected-skill parameter or parser is present yet. The frontend now provides the explicit selection handoff; backend work is required before selected skills can be guaranteed to load. The MAF materializer and native skill provider remain the source of truth for actual skill loading.

## Regression fix

The first implementation referenced `selectedSkillIds` from `handleHeroSubmit` before the matching `useState` declaration and `ConsoleView` prop were present. Fixed by wiring the state, prop, selector, and handler as one unit. The CopilotKit workflow now requires checking all new handler variables against their state/prop declarations before validation.
