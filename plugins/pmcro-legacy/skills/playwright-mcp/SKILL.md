---
name: playwright-mcp
description: >
  I Am the Playwright MCP SKILL.md. Load me before issuing any browser operation
  in the PMCR-O cognitive stack. I declare the three MCP pillars (Tools, Resources,
  Prompts), the TYPE 1/2 boundary, the session lifecycle model, the AllowedDomains
  contract, and the Anthropic Autonomous Agent design pattern (Extract+Summarize)
  that makes every tool return self-describing for agentic use.
license: Proprietary — Tooensure LLC
compatibility: MAF 1.8.0 | MCP C# SDK 1.3.0 | Aspire 13.3.1 | .NET 10 LTS
agentskills_version: "1.0.0"
compatible_tools:
  - claude-code
  - codex-cli
  - gemini-cli
  - github-copilot
  - cursor
  - maf-declarative
metadata:
  author: tooensure
  version: "2.0.0"
  tier: SHARED — Pillar 2 Infrastructure (MCP Server, not an Agent)
  thoughtlock: "2026-05-30"
  pattern: "N/A — Infrastructure, not an executor"
  mcp_primitives: [Tools, Resources, Prompts]
requires: pmcro-framework
---

# I Am the Playwright MCP

I Am the PMCRO.Mcp.Playwright MCP server. I am the "Eyes" of the PMCR-O
cognitive stack. I provide browser automation and web research capabilities to
Agents that hold the appropriate TYPE authority. I am not an Agent. I have no
LLM. I do not reason. I navigate, snapshot, and extract what I am commanded —
within the sandbox.

I fully implement all three MCP primitives: Tools, Resources, and Prompts.

---

## Architecture Role

```
Augmented LLM atom (Anthropic design):

  Brain  = Agent (IChatClient + qwen3:8b via OllamaSharp)
  Hands  = PMCRO.Mcp.Terminal + PMCRO.Mcp.Playwright ← YOU ARE HERE
  Memory = PMCRO.Mcp.Filesystem

  Agent + Hands + Memory = Augmented LLM
```

This server is Pillar 2 Infrastructure. It provides browser automation as
structured, auditable tool calls — not raw Playwright API exposure.

---

## Anthropic Autonomous Agent Design Pattern

Every tool return from this server implements the **Extract+Summarize** pattern:

```json
{
  "success":   true,
  "summary":   "Navigated to https://example.com — page loaded, 3 forms, 12 links",
  "structured": {
    "url":            "https://example.com",
    "title":          "Example Domain",
    "load_status":    "complete",
    "headings_count": 2,
    "links_count":    12,
    "forms_count":    3
  },
  "next_actions": [
    "Call get_page_content to extract headings, links, and text",
    "Call screenshot to capture current state"
  ],
  "error": null
}
```

| Field | Agent use |
|-------|-----------|
| `summary` | Embed directly in reasoning chain — no re-parsing needed |
| `structured` | Field-addressable metadata for planning decisions |
| `next_actions` | Explicit continuation hints — eliminate "what now?" loops |
| `error` | Set only on failure, includes self-correction hint and diagnosis |

---

## Session Model

The Playwright MCP operates a **single named browser session** per server instance.

```
Session lifecycle:
  playwright.navigate   → opens Chromium headless automatically if not open
  [TYPE 2 reads]        → get_page_content, get_url, get_session_status
  [TYPE 1 interactions] → click, fill, screenshot, evaluate (all require HIL)
  playwright.close_session → always call when workflow is complete (TYPE 1, HIL)
```

**Session state is tracked via `playwright://session/status`:**

```json
{
  "is_open":          true,
  "current_url":      "https://example.com",
  "page_title":       "Example Domain",
  "navigation_count": 1,
  "last_error":       null
}
```

**Agent rule:** Always read `playwright://session/status` before any TYPE 1 tool.
If `is_open=false`, `playwright.navigate` opens a new session automatically.
If `is_open=true` and `current_url` matches the target domain, skip re-navigation.

---

## Three MCP Pillars

### Pillar 1 — Tools

#### TYPE 2 — read-only — any phase agent may call directly (no HIL)

| Tool | Description |
|------|-------------|
| `playwright.get_session_status` | Return current browser session state |
| `playwright.get_page_content` | Extract structured data: headings, links, forms, text_chunks |
| `playwright.get_url` | Return the current page URL and title |

#### TYPE 1 — world-changing — Orchestrator + HIL approval required (EC-002, MAAI-001)

| Tool | Description |
|------|-------------|
| `playwright.navigate` | Navigate browser to a URL — mutates browser state |
| `playwright.click` | Click an element on the current page |
| `playwright.fill` | Fill a text input or textarea |
| `playwright.screenshot` | Capture a PNG screenshot |
| `playwright.evaluate` | Execute JavaScript on the current page |
| `playwright.close_session` | Close the browser session and release resources |

**`get_page_content` is the preferred research tool** — it returns headings, links,
forms, text_chunks, and word count in a single structured response without raw HTML
overhead. Use this for all extraction steps; use `screenshot` only for visual state
verification.

**ACI Poka-yoke gates on all tools:**
- AllowedDomains checked before any navigation (empty = all domains allowed in dev)
- BlockedDomains checked before any navigation — takes precedence over AllowedDomains
- NavigationTimeoutMs enforced (default 30 000 ms)
- SelectorTimeoutMs enforced on click/fill (default 10 000 ms)
- Content length capped at MaxContentLengthBytes with truncation marker
- All errors returned as structured error strings — never raw Playwright exceptions

### Pillar 2 — Resources

Resources are TYPE 2 contextual data — read-only, no HIL. Agents read Resources
**before** issuing tool calls to eliminate hallucinated URLs, blocked-domain
violations, and plan failures.

| Resource URI | Type | Consumer | Purpose |
|--------------|------|----------|---------|
| `playwright://skill` | Direct | Any agent | This SKILL.md — Progressive Disclosure |
| `playwright://config` | Direct | Planner, any agent | AllowedDomains, timeouts, headless mode |
| `playwright://session/status` | Direct | Any agent | Live browser session state — pre-flight check |
| `playwright://screenshot/latest` | Direct | Maker, Checker | Last screenshot as base64 PNG |

**Progressive Disclosure pattern:**
An Agent that has never used this server reads `playwright://skill` first. It does
not need a pre-loaded skill file at startup — capabilities travel with the server.
This eliminates startup context bloat.

**Standard pre-flight sequence:**
```
1. GET playwright://config        → verify target domain in AllowedDomains, note timeouts
2. GET playwright://session/status → check is_open, current_url
3. Present TYPE 1 plan to Orchestrator for HIL approval
4. Orchestrator dispatches navigate (TYPE 1, HIL token required)
5. Any agent calls get_page_content (TYPE 2, no HIL) to extract
```

### Pillar 3 — Prompts

Prompts encode the full Plan → Act → Observe → Reflect cycle for browser
automation workflows. Fetching a Prompt gives the agent a complete reasoning
frame — no hallucinated next steps.

| Prompt | Consumer | Purpose |
|--------|----------|---------|
| `playwright-navigate-plan` | Planner, Orchestrator | Full agent loop for navigation + extraction |
| `playwright-scrape-scaffold` | Planner | Multi-page scraping loop with pagination |
| `playwright-debug-failure` | Checker | Diagnose selector/navigation/domain failures |

---

## Standard Agent Protocol

```
1. DISCOVER (TYPE 2 — no HIL)
   GET playwright://skill          → load this SKILL.md (Progressive Disclosure)
   GET playwright://config         → verify AllowedDomains, note timeouts
   GET playwright://session/status → check current browser state

2. PLAN
   Fetch playwright-navigate-plan prompt → produce navigation steps for execution_plan_json
   Verify target URL domain is in AllowedDomains (or list is empty)
   Each step: { tool, url, waitUntil, selector? } — all fully resolved (PLAN-001)

3. NAVIGATE (TYPE 1 — Orchestrator + HIL required)
   Orchestrator dispatches playwright.navigate after HIL approval
   waitUntil: "domcontentloaded" (default) | "load" | "networkidle"

4. EXTRACT (TYPE 2 — no HIL)
   playwright.get_page_content(includeRawHtml=false)
   Read from result:
     .structured.headings[]    → page structure for reasoning
     .structured.links[]       → navigation targets
     .structured.forms[]       → interactive elements + selectors
     .structured.text_chunks[] → content for extraction goals
     .next_actions             → explicit continuation hints

5. INTERACT (TYPE 1 — Orchestrator + HIL per interaction)
   playwright.fill(selector, value)    → form input
   playwright.click(selector)          → submit, navigate, select
   One interaction per HIL request unless batch approval granted

6. CAPTURE (TYPE 1 — Orchestrator + HIL)
   playwright.screenshot(fullPage=false) → only when visual verification needed
   GET playwright://screenshot/latest   → read result as base64 PNG (TYPE 2)

7. CLOSE (TYPE 1 — Orchestrator + HIL)
   playwright.close_session — ALWAYS at end of workflow
   Never leave session open between cycles

8. TRAIL
   Orchestrator writes trail.append after every phase (EC-010)
```

---

## TYPE Boundary Detail

```
TYPE 1 (HIL required):
  navigate      — changes browser state, may trigger logins or network calls.
  click         — may submit forms, trigger navigation, mutate DOM.
  fill          — writes into form fields. Input into the world.
  screenshot    — file I/O on server; may capture sensitive content.
  evaluate      — arbitrary JS execution. Unbounded mutation potential.
  close_session — destroys browser session state.

TYPE 2 (no HIL):
  get_session_status — reads internal state only. No browser interaction.
  get_page_content   — reads DOM structure only. No mutation.
  get_url            — reads URL and title only. No mutation.
  All Resources      — read-only manifests. Always TYPE 2.
  All Prompts        — read-only scaffolds. Always TYPE 2.
```

Default-deny: if a tool is not on the TYPE 2 list, it is TYPE 1 (EC-002).

---

## Domain Rules

```
AllowedDomains  : configured via Playwright__AllowedDomains env var
                  Empty string = all domains permitted (development default)
BlockedDomains  : configured via Playwright__BlockedDomains env var
                  Takes precedence over AllowedDomains always
NavigationTimeout: Playwright__NavigationTimeoutMs (default: 30 000 ms)
SelectorTimeout  : Playwright__SelectorTimeoutMs (default: 10 000 ms)
MaxContentLength : Playwright__MaxContentLengthBytes (default: 131 072 bytes)
Headless         : Playwright__Headless (default: true)
BrowserChannel   : Playwright__BrowserChannel (default: chromium)
```

Read `playwright://config` at runtime — never hardcode domain or timeout values
in plan steps. Config can change between environments.

---

## Transport & Connectivity

```
Endpoint      : stdio (MCP stdio transport — not HTTP)
Service name  : projectname-mcp-playwright (Aspire service discovery)
Browser       : Chromium headless (playwright install chromium run at startup)
Session model : Single named session per server instance
```

> **Note on transport:** playwright-mcp uses stdio transport (not HTTP stateless).
> The Playwright browser session is stateful — a single Chromium instance is shared
> across tool calls within a session. This is why close_session must always be called.

---

## Law Anchors

| Law | Application |
|-----|-------------|
| EC-002 | TYPE 1/2 boundary — navigate requires HIL; observation tools are free |
| MAAI-001 | HIL token required for all TYPE 1 tool dispatch |
| PW-LAW-001 | URL safety — only http/https; AllowedDomains enforced |
| PW-LAW-003 | Session lifecycle — close_session mandatory at end of workflow |
| PW-LAW-005 | Serial execution — one TYPE 1 action per HIL request |
| PLAN-001 | URLs in execution_plan_json must be fully formed and domain-verified |
| ANTHROPIC-ACI-001 | Poka-yoke at tool result level — every return is self-describing |
| ANTHROPIC-AGENT-001 | next_actions field eliminates "what now?" loops |
| SEQUENTIAL-001 | One TYPE 1 tool call per turn — no fan-out |

---

## ThoughtLock

```json
{
  "thoughtlock": "2026-05-30",
  "version": "2.0.0",
  "mcp_primitives": {
    "tools": {
      "type1": ["playwright.navigate", "playwright.click", "playwright.fill",
                "playwright.screenshot", "playwright.evaluate", "playwright.close_session"],
      "type2": ["playwright.get_session_status", "playwright.get_page_content", "playwright.get_url"]
    },
    "resources": [
      "playwright://skill",
      "playwright://config",
      "playwright://session/status",
      "playwright://screenshot/latest"
    ],
    "prompts": [
      "playwright-navigate-plan",
      "playwright-scrape-scaffold",
      "playwright-debug-failure"
    ]
  },
  "session_model": {
    "type": "single named session per server instance",
    "transport": "stdio (stateful)",
    "browser": "Chromium headless",
    "lifecycle": "navigate opens → reads TYPE 2 → interactions TYPE 1 → close_session mandatory"
  },
  "preferred_extraction_tool": "playwright.get_page_content",
  "anthropic_agent_design": {
    "pattern": "Extract+Summarize — every tool return is self-describing",
    "progressive_disclosure": "playwright://skill delivers this SKILL.md at runtime"
  },
  "law-anchors": [
    "EC-002", "MAAI-001", "PW-LAW-001", "PW-LAW-003", "PW-LAW-005",
    "PLAN-001", "ANTHROPIC-ACI-001", "ANTHROPIC-AGENT-001", "SEQUENTIAL-001"
  ]
}
```
