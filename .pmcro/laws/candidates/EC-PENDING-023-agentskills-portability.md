# EC-PENDING-023 — AgentSkills Portability Declaration
## Status: PENDING-HIL (HIL approved 2026-05-30) | Source: EVOLUTION-005 | Date: 2026-05-30

---

## Proposed Law

All PMCR-O SKILL.md files MUST declare the following fields in their YAML frontmatter:

```yaml
agentskills_version: "1.0.0"
compatible_tools:
  - claude-code
  - codex-cli
  - gemini-cli
  - github-copilot
  - cursor
  - maf-declarative
```

The `agentskills_version` field declares compliance with the agentskills.io open
standard (ratified December 18, 2025). The `compatible_tools` field signals to
any tool that loads this skill which platforms have been validated.

---

## Rationale

The agentskills.io open standard is now supported by 32+ tools including Claude Code,
OpenAI Codex CLI, Google Gemini CLI, GitHub Copilot, Cursor, VS Code, and MAF Declarative.
PMCR-O skill files are structurally compliant (folder + SKILL.md + YAML frontmatter +
markdown instructions) but do not explicitly signal that compliance.

Adding these two fields costs nothing. It makes PMCR-O skills discoverable and
portable across the entire ecosystem — including MAF's Declarative package, which
natively parses SKILL.md YAML block scalars as of 1.6.0-rc1.

Cross-tool portability is an industry expectation as of 2026, not optional.

---

## Scope

All files in `A:\PMCR-O\skills\**\SKILL.md`:
- pmcro-framework
- orchestrator-agent
- planner-agent
- maker-agent
- checker-agent
- reflector-agent
- cognitive-trails
- framework-evolution
- terminal-mcp
- Any future skills

---

## Fracture

Failure to declare these fields is a documentation fracture:
`FRAC-AGENTSKILLS-PORTABILITY-001`

---

## Source

- agentskills.io specification v1.0.0
- EarnedConstraint EVOLUTION-005, evolution-2026-05-30/earned-constraints.md
- Firecrawl Agent Skills explainer (2026-05-22)

---

*PENDING-HIL (approved 2026-05-30) | EC-PENDING-023 | © 2026 Tooensure LLC*
