---
name: filesystem-mcp
description: >
  I Am the Filesystem MCP skill. Load me when any agent needs to understand the
  filesystem server's tool contract, TYPE 1/2 boundary, sandbox rules, Resource URIs,
  or Prompt scaffolds before issuing filesystem tool calls. Use for file read, write,
  delete, move, list, stat, and sandbox navigation. Load me before planning any step
  that touches the filesystem.
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
  version: "3.0.0"
  tier: SHARED — Pillar 3 Infrastructure
  thoughtlock: "2026-05-31"
  pattern: "N/A — MCP capability doc, not an executor"
  mcp-server: projectname-mcp-filesystem
  disclosure: >
    Stage 1 (this file): TYPE boundary summary and quick-start.
    Stage 3 (read_skill_resource): full tool signatures, Resource URIs,
    Prompt scaffolds, agent protocol, colony laws — see references/FULL_CONTRACT.md
---

# I Am the Filesystem MCP Skill

I Am the Filesystem MCP capability document. I give agents the contract they need
to use the `projectname-mcp-filesystem` server correctly.

## Quick-Start (memorise before any filesystem tool call)

```
TYPE 2 (call freely — no HIL):
  filesystem.list_directory   filesystem.file_exists   filesystem.get_info
  Resources: filesystem://roots   filesystem://config   filesystem://skill
             filesystem://stat/{path}

TYPE 1 (Orchestrator + HIL required — EC-002):
  filesystem.read_file   filesystem.write_file
  filesystem.delete_file filesystem.move_file
```

Pre-flight sequence (run before any operation):
1. `GET filesystem://roots` → confirm your target path is under an AllowedRoot
2. `GET filesystem://config` → know the limits (MaxFileSizeBytes, MaxListEntries)
3. `filesystem.file_exists(path)` → verify before reading

Every tool returns `.summary` (reason), `.structured` (act), `.next_actions` (navigate).
Read `.summary` to reason. Read `.structured` to act. Follow `.next_actions`.

## Full Contract

For full tool signatures, Resource URI details, Prompt scaffold names, agent protocol,
and colony law anchors:

```
read_skill_resource("filesystem-mcp", "references/FULL_CONTRACT.md")
```

## ThoughtLock

```json
{
  "thoughtlock": "2026-05-31",
  "version": "3.0.0",
  "disclosure-model": "MAF native progressive disclosure — Stage 1 here, Stage 3 in references/",
  "law-anchors": ["EC-002", "SAFETY-FS-001", "MAAI-001", "EC-MAF-SKILLS-001"]
}
```
