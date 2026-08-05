# Desktop Commander — Capability Reference

Loaded by MAF AgentSkillsProvider via `read_skill_resource("desktop-commander", "references/capabilities.md")`.
Used by the Checker when auditing Maker output against the capability contract.

## Filesystem

### read_file
- **Type**: TYPE2 (read-only)
- **Parameters**: `path` (required), `offset` (optional), `length` (optional), `isUrl` (optional)
- **Returns**: File contents as text, or base64 for images
- **Supports**: .txt, .md, .json, .xml, .csv, .pdf, .docx, .xlsx, .png, .jpg, .gif, .webp

### write_file
- **Type**: TYPE1 (mutating — HIL gate)
- **Parameters**: `path` (required), `content` (required), `mode` ("rewrite"|"append")
- **HIL**: Pauses at HIL decision point before dispatch
- **Constraint**: Maximum 50 lines per call per server config

### list_directory
- **Type**: TYPE2
- **Parameters**: `path` (required), `depth` (optional, default 2)
- **Returns**: [DIR] and [FILE] prefixed entries with relative paths

### create_directory
- **Type**: TYPE1
- **Parameters**: `path` (required)
- **Creates**: Nested directories in one operation

### move_file
- **Type**: TYPE1
- **Parameters**: `source` (required), `destination` (required)
- **Destructive**: Overwrites destination — confirm before calling

### edit_block
- **Type**: TYPE1
- **Parameters**: `file_path` (required), `old_string` (required), `new_string` (required), `expected_replacements` (optional)
- **Strategy**: Small, focused edits preferred over large rewrites

### start_search
- **Type**: TYPE2
- **Parameters**: `path` (required), `pattern` (required), `searchType` ("files"|"content")

### get_file_info
- **Type**: TYPE2
- **Parameters**: `path` (required)
- **Returns**: size, dates, permissions, line count, Excel sheet info

## Terminal

### start_process
- **Type**: TYPE1
- **Parameters**: `command` (required), `timeout_ms` (required), `shell` (optional)
- **Default shell**: powershell.exe on Windows

### interact_with_process
- **Type**: TYPE1
- **Parameters**: `pid` (required), `input` (required)

### read_process_output
- **Type**: TYPE2
- **Parameters**: `pid` (required), `offset` (optional), `length` (optional)

## Browser

### scrape
- **Type**: TYPE2
- **Parameters**: `url` (required), `includeMarkdown` (optional)
- **Returns**: Page text content, JSON-LD, head metadata

### google_search
- **Type**: TYPE2
- **Parameters**: `q` (required), plus optional filters (site, filetype, before, after, etc.)

## HIL Gating Rules

| Rule | Applies To |
|---|---|
| TYPE1 actions pause at HIL decision point | write_file, create_directory, move_file, edit_block, start_process, interact_with_process, force_terminate, kill_process, write_pdf |
| TYPE2 actions dispatch directly | read_file, list_directory, start_search, get_file_info, read_process_output, list_sessions, list_processes, scrape, google_search, get_config |
| HIL deny → cycle Halts | Maker never self-executes a denied mutation |
| HIL approve → dispatch + Checker audits post-dispatch state | Checker never audits unresolved TYPE1_PENDING stubs |