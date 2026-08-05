# EC-PENDING-022 — FIDES Integrity Labels
## Status: PENDING-HIL | Source: EVOLUTION-002 | Date: 2026-05-30

---

## Proposed Law

When MAF FIDES middleware (Flow Integrity Deterministic Enforcement System)
is adopted into the PMCR-O stack, the following rules apply:

1. All content entering the Maker from external MCP sources (search, filesystem,
   browser, any remote server) MUST carry `integrity: untrusted` label through
   the FIDES middleware pipeline.

2. The Checker MUST validate label propagation as part of law compliance scoring
   (LawComplianceScore). Any `integrity: trusted` content that entered via an
   external MCP source without explicit Orchestrator declaration in the execution
   plan is a law fracture → LOOP.

3. Content carrying `integrity: trusted` requires one of:
   - Explicit HIL token in `X-HIL-Approval-Token` header (MAAI-001), OR
   - Orchestrator declaration in execution_plan_json: `"trust_override": true`
     with a stated reason.

4. The TYPE 1/2 boundary (EC-002) is extended — not replaced — by FIDES.
   FIDES adds a runtime enforcement layer on top of the static allowlist.

---

## Rationale

MAF 1.8.0 ships FIDES as first-class middleware. FIDES defends against prompt
injection at the framework layer — not a system prompt heuristic, but a
propagating label on every content unit. This makes the TYPE 1/2 boundary
dynamic: content can be TYPE 2 (read-only) but still carry `integrity: untrusted`
if it originated from an external source.

PMCR-O's Checker currently scores three dimensions: correctness, law compliance,
completeness. FIDES integrity label propagation is a fourth law compliance check
that can be added without restructuring the existing scoring model.

---

## Adoption Gate

Do NOT adopt until:
- [ ] FIDES middleware documentation is stable in MAF devblog
- [ ] `Microsoft.Agents.AI.Middleware.Fides` NuGet package is non-preview
- [ ] Operator (Shawn) reviews and approves this draft

---

## Source

- Microsoft Agent Framework devblog, May 20, 2026: FIDES announcement
- EarnedConstraint EVOLUTION-002, evolution-2026-05-30/earned-constraints.md

---

*PENDING-HIL | EC-PENDING-022 | © 2026 Tooensure LLC*
