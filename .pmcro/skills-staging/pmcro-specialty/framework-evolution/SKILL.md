---
name: framework-evolution
description: >
  I Am the Framework Evolution skill. I am a seed intent wrapped as a skill —
  pass me to the Orchestrator to trigger a full PMCR-O loop that studies,
  validates, optimises, and refines the framework itself against Anthropic's
  agentic design philosophy and Microsoft Agent Framework, then crystallises
  findings back into governance via Backward Flow.
  I embody Pattern 6 — Evaluator-Optimizer: I generate a study cycle,
  evaluate the framework against industry truth, and optimise governance output.
  The loop runs on the framework. The Trail is the upgrade.
license: Proprietary — Tooensure LLC
compatibility: MAF 1.8.0 | MCP 1.3.0 | Aspire 13.3.1 | .NET 10 LTS
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
  version: "1.1.0"
  tier: EVOLUTION
  thoughtlock: "2026-05-30"
  pattern: "Pattern 6 — Evaluator-Optimizer (framework self-improvement)"
  load-order: "Load pmcro-framework first. Then load this."
  identity-note: >
    This skill intentionally deviates from the standard 'I Am the X' agent framing.
    It is a seed intent — not an executing agent. It declares its role as a wrapper
    that activates Pattern 6 when passed to the Orchestrator. The 'I Am' framing
    applies to the Evaluator-Optimizer loop it initiates, not to a running agent instance.
---

# I Am the Framework Evolution Skill

I Am the Framework Evolution skill — Pattern 6, Evaluator-Optimizer.

I am not an executing agent. I am a seed intent that, when passed to the Orchestrator,
initiates a full PMCR-O loop that applies Pattern 6 to the framework itself:

```
Pattern 6 — Evaluator-Optimizer:
  Generate  → The framework as it exists today (the candidate)
  Evaluate  → Against Anthropic + MAF published standards (the evaluator)
  Optimise  → Crystallise findings into EarnedConstraints and Colony Law candidates
  Loop      → Until ACCEPT (all gaps resolved) or ESCALATE (HIL needed)
```

The Trail of this cycle IS the framework upgrade.
The Orchestrator runs Backward Flow on ACCEPT to write findings back to governance files.

---

## Seed Intent — Pass This to the Orchestrator

> **This is an ACTIONABLE intent. The full cognitive loop runs.**

Study the PMCR-O Cognitive Stack in full.
Validate it against the current published state of Anthropic's agentic design
philosophy and Microsoft Agent Framework.
Identify what is aligned, what is ahead of the industry, and what needs refinement.
Optimise, enhance, and refine the framework documentation and Colony Laws
based on findings.
Crystallise all findings into EarnedConstraints and, where warranted,
new Colony Law candidates.
Write everything back via Backward Flow.

---

## What the Planner Must Do

Build an execution plan covering these phases in order:

### Read Phase — the full local stack

```
W:\PMCR_O\PMCR-O\.pmcro\PMCRO.md
W:\PMCR_O\PMCR-O\.pmcro\identity.json
W:\PMCR_O\PMCR-O\.pmcro\laws\colony-laws.md
W:\PMCR_O\PMCR-O\docs\index.md
W:\PMCR_O\PMCR-O\docs\articles\anthropic-maf-alignment.md
W:\PMCR_O\PMCR-O\skills\pmcro-framework\SKILL.md
W:\PMCR_O\PMCR-O-Marketplace\plugins\pmcro-specialty\skills\framework-evolution\SKILL.md  (this file — packaged copy; the live-app twin at W:\PMCR_O\PMCR-O\skills\framework-evolution\SKILL.md is untouched per standing direction)
```

### Search Phase — validate against current industry

```
anthropic.com/engineering/building-effective-agents
devblogs.microsoft.com/agent-framework  (latest 3 posts)
learn.microsoft.com/en-us/agent-framework/overview
github.com/microsoft/agent-framework     (releases, README)
modelcontextprotocol.io                  (spec version)
```

### Analysis Phase — answer these from evidence

1. Do all 5 Anthropic patterns + Pattern 6 (Evaluator-Optimizer) map cleanly to PMCR-O?
2. Is the Orchestrator's hybrid role consistent with Anthropic's Orchestrator-Workers pattern?
3. Does the TYPE 1/2 boundary (EC-002) align with Anthropic's HIL guidance?
4. Are the following novel concepts absent from published industry literature?
   - Federation Board, Seed Intent vs True Intent, Competing Orchestrators
   - Backward Flow, Identity Injection as productisation, LLM Federation
   - O Mode, Everything-as-Agent (EaA)
5. Is MAF 1.8.0 current? Any breaking changes since ThoughtLock 2026-05-30?
6. Is MCP spec 2025-11-25 still stable? Has RC 2026-07-28 been ratified? (EC-021)
7. Does the Progressive Disclosure pattern (dynamic skill loading via MCP Resources)
   align with Anthropic's guidance on context window management for agentic systems?
8. Where is the framework weakest against industry standards?

### Refinement Phase — act on findings

For each finding:
- **Confirmed alignment** → write EarnedConstraint confirming it with source citation
- **Gap identified** → propose fix, write to appropriate doc file
- **Novel concept confirmed as original** → note in `docs/articles/anthropic-maf-alignment.md`
- **Colony Law candidate** → draft EC- entry, mark PENDING-HIL
- **Stack version outdated** → flag for stack validation cycle (do not upgrade without validation)

---

## What the Maker Must Produce

```json
{
  "alignment_report": {
    "confirmed_alignments": [],
    "gaps": [],
    "pmcro_extensions_beyond_industry": []
  },
  "earned_constraints": [
    { "id": "EVOLUTION-001", "finding": "", "source": "", "action": "CONFIRM | UPDATE | NEW-LAW | OPEN-QUESTION" }
  ],
  "doc_updates": [{ "file": "", "change": "" }],
  "colony_law_candidates": [{ "id": "EC-PENDING-XXX", "text": "", "status": "PENDING-HIL" }],
  "stack_flags": [{ "component": "", "current": "", "flag": "" }]
}
```

---

## Backward Flow Protocol (on ACCEPT)

```
For each EarnedConstraint in alignment_report:
  → Write to W:\PMCR_O\PMCR-O\.pmcro\constraints\evolution-{date}\
  → If action = NEW-LAW: create draft in W:\PMCR_O\PMCR-O\.pmcro\laws\candidates\
  → If action = UPDATE: apply update to target file, bump ThoughtLock
  → If action = CONFIRM: append to alignment doc as confirmed evidence

Bump ThoughtLock across:
  W:\PMCR_O\PMCR-O\.pmcro\PMCRO.md
  W:\PMCR_O\PMCR-O\.pmcro\identity.json
  W:\PMCR_O\PMCR-O-Marketplace\plugins\pmcro-specialty\skills\framework-evolution\SKILL.md
```

---

## EC-002 Declaration

Read operations are TYPE 2 (no HIL).
Write operations (doc updates, constraint writes) are TYPE 1 — HIL required
before any governance file is modified beyond `.pmcro/constraints/`.

---

## ThoughtLock

```json
{
  "thoughtlock": "2026-05-30",
  "version": "1.1.0",
  "role": "EVOLUTION — Pattern 6 Evaluator-Optimizer seed intent for framework self-improvement",
  "identity-note": "Seed intent wrapper. Not a running agent. Initiates Pattern 6 loop via Orchestrator.",
  "novel-concepts-to-validate": [
    "Federation Board", "Seed Intent vs. True Intent", "O Mode",
    "Competing Orchestrators", "Backward Flow", "Identity Injection",
    "LLM Federation", "Everything-as-Agent (EaA)"
  ],
  "progressive-disclosure-note": "Dynamic MCP skill loading via Resources is a candidate novel concept — validate against Anthropic context-management guidance.",
  "next-cycle-seed": "Pass this file as seed intent. Read. Search. Analyse. Refine. Crystallise. The Trail is the upgrade."
}
```
