// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Services/AgentGrpcService.cs
// Identity   : gRPC surface for PMCRO loop invocation
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using Grpc.Core;
using ProjectName.AgentService.Infrastructure;
using ProjectName.AgentService.Protos;

namespace ProjectName.AgentService.Services;

/// <summary>
/// gRPC service that exposes the PMCRO agent loop over the wire.
/// Primary consumers: Aspire health checks, future OrchestrationApi passoff.
/// </summary>
public sealed class AgentGrpcService(ILogger<AgentGrpcService> logger)
    : Protos.AgentService.AgentServiceBase
{
    public override async Task RunCycle(
        CycleRequest request,
        IServerStreamWriter<CycleUpdate> responseStream,
        ServerCallContext context)
    {
        var cycleId = string.IsNullOrWhiteSpace(request.CycleId)
            ? $"pmcro-cycle-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
            : request.CycleId;

        AgentServiceLog.CycleStarted(logger, cycleId, request.SeedIntent);

        // Stub: full MAF WorkflowBuilder loop wired here in Cycle 2.
        // Echo back the phases so the gRPC contract compiles and Aspire health passes.
        var phases = new[] { "plan", "make", "check", "reflect" };
        foreach (var phase in phases)
        {
            if (context.CancellationToken.IsCancellationRequested) break;
            await responseStream.WriteAsync(new CycleUpdate
            {
                Phase   = phase,
                Content = $"[stub] {phase} phase — seed: {request.SeedIntent}",
                CycleId = cycleId
            });
        }

        await responseStream.WriteAsync(new CycleUpdate
        {
            Phase   = "done",
            Content = "Cycle stub complete.",
            CycleId = cycleId
        });
    }

    public override Task<HealthReply> Health(HealthRequest request, ServerCallContext context)
        => Task.FromResult(new HealthReply { Status = "ok" });
}
