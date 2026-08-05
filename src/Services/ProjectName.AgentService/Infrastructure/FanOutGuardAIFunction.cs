// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Infrastructure/FanOutGuardAIFunction.cs
// Identity   : AIFunction wrapper — semaphore-gated per-turn single dispatch
// Law Anchor : EC-004 (no fan-out), FRAC-ORCH-FANOUT-003
// ThoughtLock: 2026-05-31
//
// ROOT CAUSE (FRAC-ORCH-FANOUT-003):
//   SingleToolCallChatClient intercepts at the IChatClient boundary, rewriting
//   the response to strip extra FunctionCallContent blocks before
//   FunctionInvokingChatClient sees them. This works for MEA's own
//   FunctionInvokingChatClient. MAF 1.8's version, however, calls its inner
//   client through an internal virtual path that bypasses DelegatingChatClient
//   overrides, meaning SingleToolCallChatClient.GetResponseAsync never fires.
//   Three WriteFile calls still dispatch concurrently.
//
// FIX:
//   Wrap every AIFunction delegate with FanOutGuardAIFunction before handing
//   them to ChatOptions.Tools. All orchestrator tools share ONE SemaphoreSlim(1,1).
//   When FunctionInvokingChatClient invokes them:
//     - First call in a batch: acquires semaphore, executes normally.
//     - Subsequent calls (fan-out victims): semaphore already held,
//       WaitAsync times out immediately (0ms), returns suppression message.
//   The semaphore is released via a 500ms delayed Task so the next agent turn
//   starts with a clean slot regardless of how long the actual tool takes.
//
// This fix is bulletproof regardless of FunctionInvokingChatClient internals
// because it intercepts at actual AIFunction.InvokeCoreAsync execution time,
// not at the IChatClient response boundary.
//
// API NOTE (MEA 10.6.0):
//   AIFunction no longer has a Metadata property or AIFunctionMetadata type.
//   Name, Description, and JsonSchema are direct properties on AIFunction.
//   JsonSchema type is JsonElement (not JsonObject).
//   InvokeCoreAsync returns ValueTask<object?>.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;
using System.Text.Json;

namespace ProjectName.AgentService.Infrastructure;

/// <summary>
/// Wraps an <see cref="AIFunction"/> with a shared <see cref="SemaphoreSlim"/>(1,1).
/// The first invocation per agent turn acquires the semaphore and executes normally.
/// Any concurrent invocations (fan-out) receive an immediate suppression message.
/// </summary>
internal sealed class FanOutGuardAIFunction : AIFunction
{
    private readonly AIFunction    _inner;
    private readonly SemaphoreSlim _gate;

    // Release delay (ms) — long enough for FunctionInvokingChatClient to finish
    // its current dispatch loop before the next agent turn starts.
    private const int ReleaseDelayMs = 500;

    private FanOutGuardAIFunction(AIFunction inner, SemaphoreSlim gate)
    {
        _inner = inner;
        _gate  = gate;
    }

    /// <summary>
    /// Wraps <paramref name="fn"/> with the shared fan-out gate.
    /// All tools passed to a single orchestrator agent must share the SAME
    /// <paramref name="gate"/> instance.
    /// </summary>
    public static FanOutGuardAIFunction Wrap(AIFunction fn, SemaphoreSlim gate)
        => new(fn, gate);

    // ── AIFunction surface (MEA 10.6.0) ──────────────────────────────────────

    public override string      Name        => _inner.Name;
    public override string      Description => _inner.Description;
    public override JsonElement JsonSchema  => _inner.JsonSchema;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        CancellationToken cancellationToken)
    {
        // Try to acquire immediately (timeout = 0).
        bool acquired = await _gate.WaitAsync(millisecondsTimeout: 0, cancellationToken)
            .ConfigureAwait(false);

        if (!acquired)
        {
            // Fan-out victim — return suppression message, do not invoke inner.
            return $"[FAN-OUT SUPPRESSED] Tool '{_inner.Name}' was called concurrently. " +
                   "Only ONE tool call is allowed per turn. Call it again in your next response.";
        }

        // Acquired — schedule release after delay regardless of outcome.
        _ = Task.Delay(ReleaseDelayMs, CancellationToken.None)
            .ContinueWith(_ => _gate.Release(),
                TaskContinuationOptions.ExecuteSynchronously);

        try
        {
            return await _inner.InvokeAsync(arguments, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            // Release immediately on exception — don't block the next turn.
            throw;
        }
    }
}
