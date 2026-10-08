# Desktop Commander — MAF Agent Skill

Desktop Commander is the MCP provider for PMCR-O's capability layer. It exposes
filesystem, terminal, browser, search, and process management through a single
MCP server. This skill defines the canonical capability contract — the Maker
dispatches to capabilities (Filesystem, Terminal, Browser), not to Desktop
Commander directly. A binding maps each capability to Desktop Commander's tools.

## Colony Laws

| Law | Description |
|---|---|
| Law 1 — Capability Abstraction | The Maker dispatches to Fileystem/Terminal/Browser, never to Desktop Commander. The binding handles translation. |
| Law 2 — Single Provider | Desktop Commander is the ONE MCP server for all local capabilities. No capability is provided by two different MCP servers simultaneously. |
| Law 3 — Progressive Disclosure | Tools are advertised first (names + descriptions). Full SKILL.md is loaded only when the capability is needed. Scripts run on demand. |
| Law 4 — Portability | Bindings are swappable. Changing from Desktop Commander to another MCP provider requires only a binding update, not a change to any PMCR-O agent. |

## Capabilities

### Filesystem
| Tool | Type | Description |
|---|---|---|
| read_file | TYPE2 | Read file contents, PDFs, DOCX, Excel, images |
| write_file | TYPE1 | Write or append to files |
| list_directory | TYPE2 | List directory contents recursively |
| create_directory | TYPE1 | Create directories |
| move_file | TYPE1 | Move or rename files and directories |
| edit_block | TYPE1 | Surgical find-and-replace edits |
| start_search | TYPE2 | Search files by name or content |
| get_file_info | TYPE2 | File metadata (size, dates, line count) |
| read_multiple_files | TYPE2 | Read multiple files in one call |

### Terminal
| Tool | Type | Description |
|---|---|---|
| start_process | TYPE1 | Start a terminal process (shell, REPL) |
| interact_with_process | TYPE1 | Send input to running process |
| read_process_output | TYPE2 | Read output from running process |
| force_terminate | TYPE1 | Kill a terminal session |
| list_sessions | TYPE2 | List active terminal sessions |
| list_processes | TYPE2 | List system processes |
| kill_process | TYPE1 | Terminate a system process by PID |

### Browser
| Tool | Type | Description |
|---|---|---|
| scrape | TYPE2 | Scrape webpage content as text/markdown |
| google_search | TYPE2 | Web search via Serper API |

### Charts & Documents
| Tool | Type | Description |
|---|---|---|
| write_pdf | TYPE1 | Create or modify PDF files |
| createPieChart | TYPE1 | Render pie charts |
| createBarChart | TYPE1 | Render bar charts |
| createLineChart | TYPE1 | Render line charts |
| createInteractiveTable | TYPE1 | Create sortable/searchable tables |
| renderLatex | TYPE1 | Render LaTeX math |

### Configuration
| Tool | Type | Description |
|---|---|---|
| get_config | TYPE2 | Read server configuration |

## Scripts

### connect.ps1
Establishes the MCP connection to Desktop Commander. Invoked by MAF's
AgentSkillsProvider when the skill is first loaded. Validates that
Desktop Commander is running and accessible before any tool call.

## Bindings

### desktop-commander.yaml
Maps each PMCR-O capability to the corresponding Desktop Commander MCP tool.
This is the ONLY file that changes when swapping MCP providers.

## References

### capabilities.md
Full capability catalog with parameter schemas, TYPE1/TYPE2 classifications,
and HIL gating rules. Loaded by the Checker when auditing Maker output.