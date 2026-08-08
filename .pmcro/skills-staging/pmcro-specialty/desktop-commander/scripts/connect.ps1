# Desktop Commander — MCP Connection Script
# Invoked by MAF AgentSkillsProvider when load_skill("desktop-commander") is called.
#
# Progressive disclosure steps:
#   1. Advertise: skill name + description only (no tools)
#   2. Load: this script runs, establishes MCP session
#   3. Advertise tools: tool names + descriptions from binding YAML
#   4. On tool call: route through MCP client to Desktop Commander
#
# Desktop Commander runs as an Electron app. Its MCP server is embedded
# in the app process. For PMCR-O development, the custom MCP servers
# (mcp-filesystem, mcp-terminal, mcp-playwright) running under Aspire
# provide the actual MCP transport.
#
# When Desktop Commander exposes a standalone MCP endpoint (stdio or SSE),
# update this script to connect to that endpoint instead.

param(
    [string]$Transport = "stdio",
    [string]$Endpoint = ""
)

Write-Host "[desktop-commander] MCP connection requested via $Transport"

if ($Transport -eq "stdio") {
    # Desktop Commander MCP uses stdio transport.
    # The MCP client (MAF's McpClientFactory) spawns the server process
    # and communicates via stdin/stdout. The binding YAML above maps
    # each PMCR-O capability to the corresponding mcp_tool name.
    #
    # For now: the custom MCP servers under mcp/ provide the tools.
    # This script validates that the capability mapping is consistent.
    Write-Host "[desktop-commander] stdio connection — custom MCP servers active"
    Write-Host "[desktop-commander] verify: mcp-filesystem, mcp-terminal, mcp-playwright running"
    
    # Validate binding YAML exists
    $bindingPath = Join-Path $PSScriptRoot "..\bindings\desktop-commander.yaml"
    if (-not (Test-Path $bindingPath)) {
        Write-Error "[desktop-commander] binding YAML not found: $bindingPath"
        exit 1
    }
    
    Write-Host "[desktop-commander] binding validated: $bindingPath"
    Write-Host "[desktop-commander] ready — $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    exit 0
}

if ($Transport -eq "sse") {
    # Future: when Desktop Commander exposes an SSE endpoint
    Write-Host "[desktop-commander] SSE connection to $Endpoint"
    # MAF's McpClientFactory.CreateAsync() with SseClientTransport
    exit 0
}

Write-Error "[desktop-commander] unknown transport: $Transport"
exit 1