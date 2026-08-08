# Runtime Smoke Results

## AppHost

- Aspire AppHost 13.4.6 started successfully.
- Aspire dashboard started with OTLP endpoint.
- OrchestratorService, OrchestratorApi, MCP Filesystem, MCP Terminal, MCP Playwright, and frontend processes were launched.

## HTTP probes

- `http://localhost:5169/health` → HTTP 200.
- `http://localhost:5169/alive` → HTTP 200.
- `POST http://localhost:5169/agui` with an empty probe body remained open for SSE processing; this confirms the route is not a simple missing-path response. A real AG-UI payload is required for a complete interaction test.
- `POST http://localhost:5169/agui/harness` with an empty probe body remained open for SSE processing; a real AG-UI payload is required for a complete interaction test.
- GET probes are not valid AG-UI tests because the protocol surface is POST/SSE.

## Frontend

- Aspire assigned the frontend a dynamic dev port in this run (`52259`), so the old screenshot port (`63881`) is not stable.
- The frontend process launched, but the short HTTP probe timed out while the dev server was compiling/responding. Source TypeScript and production Webpack builds pass; browser-level smoke testing remains pending.

## Interpretation

The service graph starts and the health/readiness endpoints work. Full production sign-off still requires a valid AG-UI request payload, fresh CopilotKit chat verification, Harness catalog-tool verification, CodeAct execution verification, and Playwright browser checks against the current Aspire-assigned frontend endpoint.
