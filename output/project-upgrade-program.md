# PMCR-O Marketplace Full Upgrade Program

## Objective

Make the PMCR-O Marketplace project production-ready, MAF-native, enterprise-ready, and current across the .NET/Aspire backend and CopilotKit frontend without breaking the preview MAF/Harness/CodeAct integration.

## Current baseline

- Target framework: `net11.0` using .NET 11 preview SDK.
- Aspire: `13.4.6`.
- Microsoft Agent Framework: primarily `1.17.0`, with related preview/alpha hosting packages.
- CopilotKit: `1.62.3`.
- Next.js: `16.2.10`; React: `19.2.7`.
- Test runner: Microsoft Testing Platform through `global.json`.
- MAF marketplace source: `.agents/plugins/marketplace.json`.
- MAF runtime staging: `.pmcro/skills-staging`.

## Workstreams

### 1. Version and dependency coherence

- Build a package compatibility matrix for .NET 11, Aspire, MAF, AG-UI, Harness, Hyperlight/CodeAct, OpenTelemetry, MCP, and CopilotKit.
- Avoid mixing incompatible MAF date-stamp families without a build and runtime check.
- Upgrade only after recording the current baseline and reviewing breaking changes.
- Keep package versions centralized in `Directory.Packages.props` and frontend versions in `src/frontend/package.json`.

### 2. MAF-native execution

- Keep `.agents/plugins/marketplace.json` as the single registry.
- Materialize registered plugin skills into `.pmcro/skills-staging`.
- Use native `AgentSkillsProvider` for Orchestrator, Harness, and CodeAct.
- Keep `SkillCatalogService` derived from unique staged `SKILL.md` names.
- Add runtime smoke tests for `load_skill`, `read_skill_resource`, and catalog queries.
- Ensure selected frontend skill names are validated against the staged catalog before being passed into a cycle.

### 3. Agent surfaces

- Orchestrator: PMCR-O Plan → Make → Check → Reflect with AG-UI state snapshots.
- Harness: MAF harness loop with read-only tools, progressive skill loading, completion marker, and iteration cap.
- CodeAct: Hyperlight Python execution with read-only MCP tools and staged MAF skills.
- Explicitly document the HIL boundary for any future CodeAct mutation support.

### 4. Enterprise hardening

- Confirm environment-specific configuration and remove reliance on development-only defaults.
- Add authenticated/authorized production boundaries where deployment requires them.
- Add request correlation, audit events, agent/tool identity, cycle IDs, and HIL decision records.
- Add rate/timeout/cancellation policies for AG-UI and long-running local-model calls.
- Review secret handling, log redaction, file-system scope, command allowlists, and CodeAct sandbox limits.
- Add health/readiness checks for Orchestrator, Ollama, MCP services, skill staging, and AG-UI.

### 5. Observability and resilience

- Verify OpenTelemetry traces, metrics, and logs in Aspire.
- Add custom spans around skill materialization, skill selection, cycle execution, HIL waits, and AG-UI streaming.
- Add retry/backoff policies only at safe transport boundaries; do not duplicate agent loops.
- Ensure hosted-service failures are handled intentionally under .NET 11 behavior.

### 6. Frontend production readiness

- Keep the redesigned operator workspace hierarchy.
- Add browser tests for desktop/tablet/mobile, skill search/grouping, selected context, chat mode switching, and phase state display.
- Make catalog counts and catalog answers come from the same authoritative source.
- Fix generated `.next/dev/types` contamination so clean production build validation is reliable.
- Add loading, error, disconnected, empty, reduced-motion, and keyboard-navigation checks.

### 7. Test and release gates

Required gates before declaring production-ready:

1. Clean restore with the pinned SDK.
2. Full .NET build with zero warnings/errors.
3. MTP test discovery and test execution.
4. Frontend source type check.
5. Clean Next.js production build.
6. MAF materialization smoke test.
7. Orchestrator AG-UI smoke test.
8. Harness AG-UI smoke test.
9. CodeAct sandbox smoke test.
10. Catalog consistency test: UI count equals backend `get_skill_catalog` count.
11. Playwright UI smoke suite.
12. HIL approval/denial test.
13. Deployment configuration review.

## Current conclusion

The project has the correct architectural direction and several implemented integrations, but it is not yet certified production-ready. The next safe implementation order is: dependency matrix → clean build/test baseline → MAF runtime smoke tests → enterprise hardening → frontend browser QA → release gate automation.
