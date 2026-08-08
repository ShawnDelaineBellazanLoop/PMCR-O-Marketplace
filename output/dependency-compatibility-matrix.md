# Dependency Compatibility Matrix

## Current pinned line

| Area | Current | Upgrade decision |
|---|---:|---|
| .NET SDK | 11.0.100-preview.3 | Keep pinned until .NET 11 stable release is selected |
| Target framework | net11.0 | Already upgraded |
| Aspire | 13.4.6 | Keep lockstep with AppHost SDK |
| MAF core | 1.17.0 | Keep current family |
| MAF hosting/AG-UI | 1.17.0 preview/alpha | Keep same date-stamp family; verify after every package change |
| MAF Harness | 1.17.0 stable | Keep; API compiles and test suite builds |
| Hyperlight/CodeAct | 1.17.0 preview + Hyperlight Python 0.5.0 | Keep; runtime smoke test still required |
| ModelContextProtocol | 2.1.0 | Keep until MCP server integration tests pass |
| OpenTelemetry | 1.17.0 | Keep aligned with .NET 11 preview line |
| CopilotKit | 1.62.3 | Keep until browser smoke tests prove a safe upgrade path |
| Next.js | 16.2.10 | Keep; production Webpack build passes |
| React | 19.2.7 | Keep aligned with Next.js 16 |

## Rule

Do not independently bump one MAF package. Upgrade MAF core, hosting, AG-UI, DevUI, Hyperlight, and Harness as a compatibility family, then rebuild and run the MTP/runtime smoke suite. Do not replace the native MAF skill provider with a custom loader.

## Current conclusion

No dependency version change is justified by the current baseline. The safe upgrade work is hardening, tests, runtime verification, and release gates first.
