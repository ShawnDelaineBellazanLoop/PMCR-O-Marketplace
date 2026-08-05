// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Infrastructure/AgentServiceLog.cs
// Identity   : Source-generated high-performance log delegates
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;

namespace ProjectName.AgentService.Infrastructure;

/// <summary>
/// LoggerMessage source-generated delegates for AgentService host events.
/// </summary>
internal static partial class AgentServiceLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "AgentService ready — listening on {Urls}")]
    public static partial void Ready(ILogger logger, string urls);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "PMCRO cycle started — CycleId={CycleId} Intent={Intent}")]
    public static partial void CycleStarted(ILogger logger, string cycleId, string intent);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning,
        Message = "PMCRO cycle escalated — CycleId={CycleId} Reason={Reason}")]
    public static partial void CycleEscalated(ILogger logger, string cycleId, string reason);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error,
        Message = "PMCRO cycle faulted — CycleId={CycleId}")]
    public static partial void CycleFaulted(ILogger logger, string cycleId, Exception ex);
}

/// <summary>
/// LoggerMessage source-generated delegates for MCP probe events in McpToolCache.
/// </summary>
internal static partial class McpProbeLog
{
    [LoggerMessage(EventId = 10, Level = LogLevel.Information,
        Message = "[MCP PROBE] {Name}: {Status}")]
    public static partial void ProbeOk(ILogger logger, string name, string status);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning,
        Message = "[MCP PROBE] {Name}: UNREACHABLE — {Message}")]
    public static partial void ProbeUnreachable(ILogger logger, string name, string message);
}
