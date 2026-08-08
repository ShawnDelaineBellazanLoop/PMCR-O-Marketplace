# Upgrade Baseline

## Date

2026-08-06

## Current stack

- .NET SDK: pinned .NET 11 preview via `global.json`.
- Target framework: `net11.0`.
- Aspire: `13.4.6`.
- MAF: `1.17.0` family with preview/alpha hosting packages.
- CopilotKit: `1.62.3`.
- Next.js: `16.2.10`.
- Test runner: Microsoft Testing Platform.

## Baseline results

- Full .NET solution build with `--no-restore`: passed.
- MTP test execution: no test projects found.
- Frontend targeted TypeScript check: passed.
- Next.js production build: recorded separately after completion.

## Findings

1. The Orchestrator project contained an obsolete embedded-resource skill glob. Runtime MAF marketplace materialization is the authoritative path; the embedded glob was removed in the first upgrade batch.
2. Frontend `tsconfig.json` included `.next/dev/types/**/*.ts`, which can include corrupted development-generated files and break production type checking. The development-generated include was removed; normal `.next/types` remains included.
3. No automated .NET test projects currently exist. Production readiness requires adding focused tests for catalog consistency, skill materialization, AG-UI contracts, HIL behavior, and agent registration.
4. Several MAF packages use preview/alpha tracks. Version upgrades must remain family-coherent and be validated against the restored API surface.

## Next batch

Add health/readiness checks, deterministic catalog/materialization tests, structured audit/correlation fields, and browser smoke coverage before changing major dependency versions.
