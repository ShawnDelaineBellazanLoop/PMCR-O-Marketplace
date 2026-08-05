# TYPE 2 Tool Allowlist — PMCR-O Runtime
## Version: 2.0.0 | ThoughtLock: 2026-05-29

Authoritative boundary enforcement for all phase agents.
Everything not on this list is TYPE 1 — requires HIL, Orchestrator dispatch only.

---

## Filesystem MCP

```
ReadFile          — read file contents
ListDirectory     — list files and directories
SearchFiles       — find files matching a pattern
GrepContent       — search within file contents
GetFileInfo       — get file/directory metadata
```

## Trail Tools (TYPE 2 — read only)

```
trail.get         — retrieve a TrailFrame by cycle_id + frame_id
trail.query       — query TrailFrames by cycle, loop, or phase
trail.list_cycles — list all cycle IDs in .pmcro/trails/
```

## Trail Tools (TYPE 1 — Orchestrator only)

```
trail.append      — write a new TrailFrame
```

## Terminal MCP (observation only)

```
Which             — check if a CLI tool exists on PATH
GetTerminalStatus — get current terminal state
```

## Terminal MCP (TYPE 1)

```
terminal.run        — execute a command
terminal.run-script — execute a script
```

## Playwright / Browser (read-only)

```
ExecuteBrowserResearch  — navigate + extract text (non-destructive)
browser_snapshot        — get accessibility tree
browser_screenshot      — capture screenshot
browser_wait_for        — wait for element or condition
GetPageTitle            — get page title
GetInnerText            — get DOM element inner text
```

## Playwright / Browser (TYPE 1 — HIL required)

```
browser_navigate · browser_click · browser_fill
browser_type · browser_press_key · browser_drag
browser_drop · browser_close
```

## Skill Tools (meta — TYPE 2)

```
load_skill            — load a SKILL.md into context
read_skill_resource   — read a resource embedded in a SKILL.md
```

---

## Decision Rule

```
Does this tool write to disk?              → TYPE 1
Does this tool delete or move files?       → TYPE 1
Does this tool execute a terminal command? → TYPE 1 (unless Which/GetTerminalStatus)
Does this tool append to the trail?        → TYPE 1
Does this tool mutate browser state?       → TYPE 1
Does this tool send mutating network requests? → TYPE 1
Is this tool read-only with no side effects?   → TYPE 2 ✓
```

If uncertain: default-deny. Treat as TYPE 1.
