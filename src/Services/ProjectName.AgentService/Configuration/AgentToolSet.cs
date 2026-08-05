// ProjectName.AgentService/Configuration/AgentToolSet.cs

namespace ProjectName.AgentService.Configuration;

/// <summary>
/// Controls which MCP tools are injected into a phase agent's ChatOptions.Tools list.
/// </summary>
internal enum AgentToolSet
{
    /// <summary>No tools — agent reasons from context only (orchestrator, auditor).</summary>
    None,

    /// <summary>TYPE 2 read-only tools: ReadFile, ListDirectory, SearchFiles, GrepContent, GetFileInfo, trail.*</summary>
    Type2Reads,

    /// <summary>Full tool set including TYPE 1 write/execute tools (maker, researcher).</summary>
    FullMaker,
}
