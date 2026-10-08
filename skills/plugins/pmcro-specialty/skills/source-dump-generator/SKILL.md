name: source-dump-generator
description: Colony skill for generating comprehensive or compact source code dumps of the PMCR-O repository. Produces single-file exports for analysis, transfer, or archival.
version: 1.0.0
source: colony-internal

## Colony Laws

1. **One Bounded Action Per Cycle**: Execute exactly one tool call per cycle. Report the raw result, never summarize or re-run.
2. **TYPE1/TYPE2 Discipline**: WriteFile is TYPE1 (mutating). It returns a TYPE1_PENDING stub. The Orchestrator handles real dispatch post-approval. Never call any write tool twice.
3. **Ground Truth Honesty**: On TYPE2 reads, report actual bytes returned. On TYPE1, report TYPE1_PENDING status honestly — do not claim verification of a stub.
4. **Truthful Tool Calls**: Never hallucinate tool results. Call the tool or output the stub; never fabricate evidence.
5. **Action Scope**: Do NOT alter or improve the planned content — copy exactly. Do NOT wrap code in extra backticks or add headers unless explicitly requested.

## Skill Package Layout

### Tools Available
- ReadFile (path): Read file content
- WriteFile (path, content): Write file (TYPE1, returns stub)
- ListDirectory (path): List directory contents
- SearchFiles (pattern, path): Search for files
- CreateDirectory (path): Create directory (TYPE1)
- ExecuteCommand (command, args): Execute shell commands

### Commands
- generate-full-dump: Creates a complete source dump including all code, configuration, and documentation files
- generate-compact-dump: Creates a compact source dump focusing on src/, mcp/, tests/, and skills/ directories

## Capabilities

This skill provides two PowerShell scripts for generating source dumps of the PMCR-O repository:

### Full Source Dump
Includes all source files across the entire repository with extensions: .cs, .csproj, .json, .md, .ts, .tsx, .props, .slnx, .xml, .yaml, .yml, .ps1

Excludes: bin/, obj/, node_modules, .git/, .pmcro/, .zip files, tmp_ files, check-harness

Output: `pmcro-source-dump.txt`

### Compact Source Dump
Focuses on essential code directories: src/, mcp/, tests/, skills/

Same file extensions and exclusions as full dump, plus: docs/, repos/, AI-Knowledge-Corpus

Output: `pmcro-compact-source-dump.txt`

Both scripts produce formatted text files with:
- Header showing generation timestamp and file count
- Separator lines between files
- Full path of each file
- Complete file content
- Error handling for unreadable files