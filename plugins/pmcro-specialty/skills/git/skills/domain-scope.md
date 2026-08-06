# Git Domain Skill

Pure, deterministic git state narration. This skill is a **tool**, not an agent — it does not know about other skills and makes no assumptions about what the caller will do with its output.

## Design Philosophy

The git skill's outputs give the Orchestrator **pointers** (paths, hashes, branch names). The Orchestrator then decides whether to hand those pointers to the filesystem skill (`read_file`, `list_project`) for deeper inspection. The git skill never makes that judgment; the filesystem skill never knows git was involved.

## Tools

| Tool | Purpose |
|---|---|
| `git_status` | Modified/staged/untracked files (relative paths only) |
| `git_log` | Recent commit history with message + hash + author + date |
| `git_diff` | Diff for a specific file or all staged/unstaged changes |
| `git_branch` | Current branch + all branches |
| `git_show` | Contents of a specific commit |

## Key Design Rule

Outputs are pointers, not conclusions. When `git_status` returns `{ "modified": ["src/ProjectName.Core/Models/TrailFrame.cs"] }`, that is just a pointer. The Orchestrator decides: "I need to understand what that file contains — call `filesystem.read_file` on it."

## Guardrails

1. Pure, deterministic — no assumptions about what the caller will do with the output.
2. Relative paths only — never absolute paths.
3. No side effects — read-only git operations only.