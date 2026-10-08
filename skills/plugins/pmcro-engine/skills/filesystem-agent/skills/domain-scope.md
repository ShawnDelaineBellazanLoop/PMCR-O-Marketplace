# Filesystem Agent Domain Skill

Pure, deterministic filesystem narration. This skill is a **tool**, not an agent — it does not know about other skills and makes no assumptions about what the caller will do with its output.

## Design Philosophy

The filesystem skill's outputs give the Orchestrator **pointers** (paths, file contents, structure). The Orchestrator then decides whether to hand those pointers to other skills (git, terminal) for deeper inspection. The filesystem skill never makes that judgment; other skills never know filesystem was involved.

## Tools

| Tool | Purpose |
|---|---|
| `list_project` | List directory structure, supports `sub_path` to scope to a subtree |
| `list_tree` | Tree view with `max_depth` to limit recursion |
| `read_file` | Read file contents, returns `line_count` and `last_modified` |
| `write_file` | Write file contents, gated with `overwrite` flag |
| `grep_content` | Search for a pattern, returns file + line number + matching line only |
| `dump_project_source` | Full source dump, returns `skipped_count` for size-filtered files |

## Key Design Rules

1. **`sub_path` scoping** — `list_project(root_path, sub_path="src")` avoids full-project scans when the Orchestrator already knows which subtree is relevant.
2. **`max_depth` limiting** — `list_tree` returns only the top N levels to avoid context overflow.
3. **`grep_content` before `dump_project_source`** — when the git skill returns `modified: ["src/X.cs"]`, the Orchestrator can immediately ask "where else does `TrailFrame` appear?" without a full source dump.
4. **`write_file` closes the loop** — read → reason → write → verify. Gated with `overwrite` flag; Checker approval should precede invocation.

## Guardrails

1. Pure, deterministic — no assumptions about what the caller will do with the output.
2. `sub_path` and `max_depth` always used to scope operations.
3. `write_file` requires explicit `overwrite` flag — never overwrites silently.
4. No side effects beyond the requested operation.