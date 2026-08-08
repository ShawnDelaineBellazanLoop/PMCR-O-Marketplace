# CopilotKit Page Split Plan

## Goal

Replace the one-page anchor dashboard with real product routes while preserving shared navigation, CopilotKit runtime wiring, MAF skill discovery, PMCR-O state, and server-side trail loading.

## Routes

- `/` — Create workspace and submit a governed PMCR-O task.
- `/harness` — Harness workspace with read-only progressive skill guidance.
- `/skills` — searchable MAF skill catalog.
- `/trails` — trail history and evidence browser.
- `/directory` — agent/domain directory.
- `/platform` — runtime, MAF, AG-UI, CodeAct, and HIL overview.

## Implemented

- Sidebar now uses route links rather than dead hash anchors.
- Skills, Trails, Harness, and Platform have dedicated pages.
- Pages share the global CopilotKit shell and use existing server loaders.
- Directory remains intact and TrailView handles incomplete dispositions.

## Acceptance criteria

- Each page has a heading, purpose, empty/error-friendly content, and responsive styles.
- Existing `/` task submission and `/directory` behavior remain intact.
- Frontend targeted TypeScript and Next.js production build pass.

## Final UI integration

- `/harness` now initializes the shared CopilotKit panel with `agentId="Harness"`.
- Other pages initialize with `Orchestrator`.
- Sidebar route state follows `usePathname()` and no longer depends on scroll-spy anchors.
