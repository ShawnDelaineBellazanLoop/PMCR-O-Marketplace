// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Infrastructure/Type1LockoutChatClient.cs
// Identity   : DelegatingChatClient — strips all tools after a TYPE 1 success
// Law Anchor : EC-002, FRAC-ORCH-LOOP-001
// ThoughtLock: 2026-05-31
//
// Problem this solves:
//   After a TYPE 1 tool (WriteFile, RunCommand, Navigate etc.) returns a success
//   result, qwen3:8b ignores the "[Done. Reply to the user now.]" cue and calls
//   another tool anyway — often the same one a second time. It then hallucinates
//   an error narrative to explain the second call. MaximumIterationsPerRequest=4
//   can't prevent this because the model still has remaining iterations.
//
// Fix:
//   This client sits OUTSIDE FunctionInvokingChatClient. It inspects every
//   ChatMessage added to the conversation. The moment it sees a tool result
//   whose text contains a Success signal from McpToolCache.CallMcp() (the
//   "[Done. Reply to the user now.]" suffix), it sets _toolsExhausted=true.
//
//   On the next GetResponseAsync/GetStreamingResponseAsync call, it clones
//   ChatOptions and sets Tools=null, forcing the model into text-only mode.
//   The model MUST produce a text reply — it has no tools to call.
//
// Pipeline position (critical):
//   rawChat
//     → Type1LockoutChatClient   ← NEW: wraps everything, strips tools post-TYPE1
//     → ToolFilterChatClient     (strips forbidden tool names)
//     → SingleToolCallChatClient (fan-out guard, inside func-invoking)
//     → FunctionInvokingChatClient (dispatches calls)
//
// Reset:
//   _toolsExhausted resets to false at the start of each new top-level request
//   (each call to GetResponseAsync from the MAF agent loop), so the next turn
//   starts clean.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;

namespace ProjectName.AgentService.Infrastructure;

/// <summary>
/// Strips all tools from ChatOptions the moment a TYPE 1 success result is
/// detected in the conversation history, preventing the model from calling
/// additional tools after a write/execute operation succeeds.
/// </summary>
internal sealed class Type1LockoutChatClient(IChatClient inner)
    : DelegatingChatClient(inner)
{
    // Signal McpToolCache.CallMcp() appends to every successful TYPE 1 result.
    private const string SuccessSignal = "[Done. Reply to the user now.]";

    private volatile bool _toolsExhausted;

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Reset at the start of each top-level turn.
        _toolsExhausted = false;

        var response = await base.GetResponseAsync(
            messages,
            BuildOptions(options),
            cancellationToken);

        // Scan the response for TYPE 1 success signals and lock out if found.
        ScanAndLock(response.Messages);

        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _toolsExhausted = false;

        await foreach (var update in base.GetStreamingResponseAsync(
            messages,
            BuildOptions(options),
            cancellationToken))
        {
            // Check each streamed content block for the success signal.
            foreach (var content in update.Contents.OfType<FunctionResultContent>())
            {
                if (content.Result?.ToString()?.Contains(SuccessSignal) == true)
                    _toolsExhausted = true;
            }

            yield return update;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ChatOptions? BuildOptions(ChatOptions? options)
    {
        if (!_toolsExhausted || options is null) return options;

        // Strip all tools — model must produce text only.
        var clone = options.Clone();
        clone.Tools = null;
        return clone;
    }

    private void ScanAndLock(IEnumerable<ChatMessage> messages)
    {
        foreach (var msg in messages)
        {
            if (msg.Role != ChatRole.Tool) continue;
            foreach (var content in msg.Contents)
            {
                var text = content switch
                {
                    FunctionResultContent frc => frc.Result?.ToString(),
                    TextContent tc            => tc.Text,
                    _                         => null,
                };
                if (text?.Contains(SuccessSignal) == true)
                {
                    _toolsExhausted = true;
                    return;
                }
            }
        }
    }
}
