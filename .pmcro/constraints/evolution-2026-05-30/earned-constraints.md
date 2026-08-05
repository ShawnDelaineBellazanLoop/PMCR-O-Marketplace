# EarnedConstraints — Framework Evolution Cycle
## Date: 2026-05-30 | Cycle: framework-evolution v1.0.0
## Status: ACCEPTED | Backward Flow: COMPLETE

All findings sourced. All law candidates marked PENDING-HIL.
No governance file modified without HIL token in this cycle.

---

## EVOLUTION-001 — Anthropic Now Documents 6 Patterns, Not 5
- **Finding:** Anthropic's current agentic design guidance defines six patterns, not five.
  The sixth pattern covers pre-flight scoping — defining success, edge cases, and trade-offs
  before execution begins.
- **Source:** MindStudio analysis of Anthropic's Building Effective Agents guide (2026-04-01);
  Augment Code agentic pattern catalog (2026-05-14).
- **PMCR-O mapping:** EC-001 (Cycle Viability Gate) + PLAN-001 (No Ambiguous Parameters)
  together implement this sixth pattern. It is present in PMCR-O but not named in docs.
- **Action:** UPDATE — add Pattern 6 to `docs/articles/anthropic-maf-alignment.md`
  under a "Pattern 6 — Pre-Flight Scoping" section. Map EC-001 + PLAN-001 explicitly.
- **Status:** PENDING-WRITE (TYPE 1 — HIL approved via "yes" on 2026-05-30)

---

## EVOLUTION-002 — MAF 1.8.0 FIDES Middleware
- **Finding:** MAF 1.8.0 ships FIDES (Flow Integrity Deterministic Enforcement System)
  as first-class middleware. Every piece of content in the agent pipeline carries an
  integrity label (trusted/untrusted) and a confidentiality label (public/private).
  Labels propagate automatically. This is defense against prompt injection at the
  framework layer — not a system prompt heuristic.
- **Source:** Microsoft Agent Framework devblog, May 20, 2026.
- **PMCR-O relevance:** FIDES directly extends the TYPE 1/2 boundary (EC-002).
  Tool results entering the Maker from external sources (search MCP, filesystem MCP)
  carry `integrity: untrusted` by default. The Checker could validate label propagation
  as a fourth scoring dimension.
- **Action:** WATCH — evaluate FIDES adoption when MAF middleware docs are stable.
  Do not modify EC-002 until evaluation complete.
- **Status:** WATCH — Colony Law candidate EC-PENDING-022

---

## EVOLUTION-003 — MAF Declarative Package
- **Finding:** `Microsoft.Agents.AI.Declarative` (1.6.0-rc1 as of 2026-05-13) is the
  official MAF implementation of template-driven, declarative workflows. It supports
  YAML block scalar parsing in SKILL.md frontmatter, sequential/concurrent/group chat/
  handoff patterns, graph-based workflows with checkpointing, and human-in-the-loop
  capabilities natively. PMCR-O describes itself as "template-driven, declarative" —
  this package is the MAF realization of that claim.
- **Source:** NuGet Gallery, Microsoft.Agents.AI.Declarative 1.6.0-rc1 (2026-05-13).
- **Action:** UPDATE — document in architecture/index.md. Evaluate adoption.
  Note: package is rc1 — do not adopt in production until stable (EC-018).
- **Status:** WATCH — evaluate when stable

---

## EVOLUTION-004 — MCP Tasks Extension → Federation Board
- **Finding:** The MCP 2026-07-28 RC includes a Tasks extension for long-running
  agent-to-agent communication via MCP. Agents can initiate tasks on other agents
  and receive async results. This is the protocol-level implementation of what
  PMCR-O calls the Federation Board — agents communicating their state to each other
  through a structured channel.
- **Source:** MCP blog, "The 2026-07-28 MCP Specification Release Candidate" (2026-05-21);
  modelcontextprotocol/experimental-ext-tasks GitHub (experimental).
- **Action:** WATCH — once MCP spec 2026-07-28 ratifies (target July 28, 2026),
  evaluate Tasks extension as the transport layer for Federation Board implementation.
  EC-021 already monitors this.
- **Status:** WATCH — tied to EC-021

---

## EVOLUTION-005 — AgentSkills Portability Frontmatter Gap
- **Finding:** The agentskills.io open standard (v1.0.0, ratified December 2025)
  recommends `agentskills_version` and `compatible_tools` fields in SKILL.md frontmatter
  for cross-tool portability signaling. PMCR-O's SKILL.md files do not include these fields.
  This means PMCR-O skills are structurally compatible with all 32+ adopters (Claude Code,
  Codex CLI, Gemini CLI, GitHub Copilot, Cursor, MAF Declarative, etc.) but do not
  signal that compatibility explicitly.
- **Source:** agentskills.io specification; firecrawl.dev Agent Skills explainer (2026-05-22);
  Paperclipped standardization article (2026-03-23).
- **Action:** UPDATE — add `agentskills_version: "1.0.0"` and `compatible_tools` to
  all SKILL.md files in `A:\PMCR-O\skills\`. Colony Law candidate EC-PENDING-023.
- **Status:** PENDING-WRITE (TYPE 1 — HIL approved via "yes" on 2026-05-30)

---

## EVOLUTION-006 — Novel Concepts Confirmed Original
- **Finding:** All seven PMCR-O novel concepts searched against Anthropic, MAF, MCP,
  agentskills.io, and broader industry literature. None found in published sources.
- **Concepts confirmed original:**
  1. Federation Board (agent self-declaration + greeting as dependency graph)
  2. Seed Intent vs. True Intent (extraction loop before main cycle)
  3. O Mode (runtime-selectable prompt engineering technique — unified concept)
  4. Competing Orchestrators (adversarial orchestration + referee)
  5. Backward Flow (EarnedConstraint graduation to permanent governance)
  6. Identity Injection as productization mechanism
  7. LLM Federation (self-governing via Checker law compliance)
- **Action:** CONFIRM — document in `docs/articles/anthropic-maf-alignment.md`
  under new "PMCR-O Extensions Beyond Industry" section.
- **Status:** PENDING-WRITE (TYPE 1 — HIL approved via "yes" on 2026-05-30)

---

## EVOLUTION-007 — MAF 1.8.0 Stack Confirmed Current
- **Finding:** NuGet confirms `Microsoft.Agents.AI` 1.8.0 as latest stable (2026-05-28).
  No breaking changes found for PMCR-O's WorkflowBuilder usage pattern.
  MAF is shipping at high cadence — weekly re-validation recommended.
- **Source:** NuGet Gallery, MicrosoftAgentFramework profile (2026-05-28).
- **Action:** CONFIRM — ThoughtLock 2026-05-30 holds. Add weekly re-validation trigger
  to stack validation schedule.
- **Status:** CONFIRMED

---

## EVOLUTION-008 — MCP Spec 2025-11-25 Confirmed Current
- **Finding:** MCP spec 2025-11-25 remains the stable production spec.
  RC 2026-07-28 locked May 21, 2026. 10-week validation window open.
  Target ratification: July 28, 2026. EC-021 correctly monitors.
  The 2025-11-25 spec continues working during transition — 12-month overlap guaranteed
  by the new deprecation policy (SEP-2596).
- **Source:** MCP blog RC announcement (2026-05-21); mcp.directory explainer (2026-05-23).
- **Action:** CONFIRM — no action required. EC-021 active.
- **Status:** CONFIRMED

---

## Colony Law Candidates (PENDING-HIL — requires operator review before adoption)

### EC-PENDING-022 — FIDES Integrity Labels
When MAF FIDES middleware is adopted into the PMCR-O stack:
All content entering the Maker from external MCP sources (search, filesystem, browser)
MUST carry `integrity: untrusted` label through the middleware pipeline.
The Checker MUST validate label propagation as a law compliance check.
Content with `integrity: trusted` label requires explicit HIL token or
Orchestrator declaration in the execution plan.
**Status: PENDING-HIL — do not adopt until FIDES docs are stable and operator reviews.**

### EC-PENDING-023 — AgentSkills Portability Declaration
All PMCR-O SKILL.md files MUST declare:
```yaml
agentskills_version: "1.0.0"
compatible_tools: [claude-code, codex-cli, gemini-cli, github-copilot, cursor, maf-declarative]
```
Cross-tool portability is an industry expectation as of December 2025.
PMCR-O skills are structurally compliant; they must now signal that compliance explicitly.
**Status: PENDING-HIL — HIL approved 2026-05-30 for frontmatter updates.**

---

*Written by: framework-evolution cycle | 2026-05-30 | PMCR-O 2.1.0*
*Backward Flow protocol: TYPE 2 reads complete. TYPE 1 writes proceed with HIL token.*
