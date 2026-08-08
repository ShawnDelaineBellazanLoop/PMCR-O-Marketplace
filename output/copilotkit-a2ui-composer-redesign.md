# CopilotKit A2UI Composer Redesign

## Problem

The console behaved like one long dashboard. The main composer sent into a hidden CopilotKit drawer, so the user could not see the submitted request or run feedback in the workspace.

## Direction

Use a structured composer workspace inspired by the A2UI Composer reference:

- quiet left navigation
- centered Create surface
- visible request/activity result surface
- explicit Context and Evidence areas
- assistant drawer as an intentional secondary surface
- soft layered background and restrained typography

## Implemented

- Visible `Latest request` activity card with Waiting, Running, Submitted, and Error states.
- Explicit Context section around routing and skill selection.
- Explicit Evidence section around Round Table and PMCR-O phase rail.
- CopilotKit remains the execution backend; the main workspace no longer depends on a hidden drawer to show submission feedback.

## Acceptance criteria

- Submitted text is visible immediately in the main workspace.
- A run status is visible while CopilotKit is processing.
- Errors are visible in the main workspace instead of only in a hidden drawer.
- The page has explicit structure rather than one undifferentiated scroll.
- Existing MAF/AG-UI routing and PMCR-O state semantics are unchanged.

## Validation

- Main workbench source now exposes submitted request, running state, and errors.
- Context and Evidence are explicit page sections.
- CopilotKit remains the secondary assistant surface and keeps the existing `/api/copilotkit` → AG-UI routing.
- Targeted TypeScript validation is being rerun after removing a stale duplicate JSX prop from the previous incremental patch.
