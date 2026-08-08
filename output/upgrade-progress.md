# Upgrade Progress

## Batch 1 — Baseline and source-of-truth cleanup

Status: **completed**

- Removed obsolete embedded-resource skill glob.
- Removed `.next/dev/types` from frontend TypeScript includes.
- Recorded baseline in [upgrade-baseline.md](upgrade-baseline.md).
- .NET build and frontend production build passed.

## Batch 2 — Test and readiness foundation

Status: **completed**

- Restored the solution's expected `tests/ProjectName.OrchestratorService.Tests` project.
- Added MTP/xUnit v3 tests for staging paths, marketplace materialization, unique catalog names, and readiness.
- Current test set: 8 tests; all pass with no analyzer warnings.
- Added `SkillStagingHealthCheck` tagged `ready`.
- Added explicit `HealthChecks:ExposeEndpoints` configuration: disabled by default, enabled in Development.
- Full solution build passes with 0 warnings and 0 errors.

## Batch 3 — Runtime and browser release gates

Status: **next**

- Start AppHost and verify `/health`, `/alive`, `/agui`, and `/agui/harness`.
- Verify MAF materialization count and `get_skill_catalog` count consistency.
- Exercise CodeAct and Harness runtime paths.
- Run CopilotKit Playwright smoke tests at desktop, tablet, and mobile widths.
- Add CI release gates after runtime checks are stable.
