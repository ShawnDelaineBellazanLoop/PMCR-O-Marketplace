# CopilotKit Project Architecture

- Frontend root: `src/frontend`.
- Shared shell: `app/layout.tsx`, `app/components/Sidebar.tsx`, and `app/components/ChatPanel.tsx`.
- Main console: `app/page.tsx` and `app/components/ConsoleView.tsx`.
- Agent directory: `app/directory/page.tsx` and agent components.
- CopilotKit bridge: `app/api/copilotkit/route.ts`.
- Global design system: `app/globals.css`.
- Browser endpoint: `/api/copilotkit`.
- Backend AG-UI target: `/agui`.

Installed frontend versions include Next.js 16, React 19, CopilotKit 1.62, and AG-UI client 0.0.57. Verify current `package.json` before changing APIs.

The browser must call the Next.js runtime route, not the backend AG-UI service directly. Server-only environment values such as `AGUI_SERVER_URL` stay in the route.

Preserve PMCR-O roles Planner, Maker, Checker, Reflector; phases Planning, Checking, Reflecting, Sealed; dispositions Accept, Retry, Halt; and agent IDs Orchestrator and Harness.

The existing design tokens and fonts are in `app/globals.css` and `app/layout.tsx`. Reuse them before adding new tokens.
