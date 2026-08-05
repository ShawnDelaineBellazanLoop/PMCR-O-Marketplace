// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Infrastructure/SingleToolCallChatClient.cs
// Identity   : DelegatingChatClient — fan-out guard + first-hop enforcer
// Law Anchor : FRAC-ROUTING-PARALLEL-001, FRAC-ORCH-FIRSTHOP-001
// ThoughtLock: 2026-05-31
//
// FRACTURE: FRAC-ORCH-FANOUT-002
//   Root cause: ChatMessage.Contents mutation (Clear + re-add) was silently
//   failing because the inner list is IList<AIContent> but some MAF versions
//   return a read-only wrapper. The fan-out guard appeared to remove extra calls
//   but FunctionInvokingChatClient still saw and dispatched them.
//   Fix: return a NEW ChatResponse / ChatResponseUpdate with only the first call.
//   Never mutate the incoming message contents.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;

namespace ProjectName.AgentService.Infrastructure;

/// <summary>
/// Two structural guards for the orchestrator's tool calls:
///
/// GUARD 1 — Fan-out (FRAC-ROUTING-PARALLEL-001 / EC-004):
///   The model may emit at most ONE tool call per response. If it emits
///   multiple, all but the first are dropped. Returns a NEW response containing
///   only the first tool call — never mutates the incoming message (FRAC-ORCH-FANOUT-002).
///
/// GUARD 2 — First-hop (FRAC-ORCH-FIRSTHOP-001):
///   The first RouteToAgent call MUST target "planner".
///   If the model routes to maker/checker/reflector as the first hop,
///   the agentName argument is rewritten to "planner" before dispatch.
///
/// Position: sits INSIDE FunctionInvokingChatClient as its direct inner client.
/// Pipeline: Type1Lockout → FunctionInvoking → SingleToolCall → ToolFilter → rawChat
/// </summary>
internal sealed class SingleToolCallChatClient(IChatClient inner)
    : DelegatingChatClient(inner)
{
    private static readonly HashSet<string> s_invalidFirstHops =
        new(StringComparer.OrdinalIgnoreCase) { "maker", "checker", "reflector" };

    private volatile bool _firstHopDispatched;

    // ── Non-streaming path ────────────────────────────────────────────────────
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await base.GetResponseAsync(messages, options, cancellationToken);
        return EnforceAndRewrite(response);
    }

    // ── Streaming path ────────────────────────────────────────────────────────
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Collect all updates, then apply fan-out + first-hop rules before yielding.
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
            updates.Add(update);

        var allCalls = updates
            .SelectMany(u => u.Contents.OfType<FunctionCallContent>())
            .ToList();

        FunctionCallContent? firstCall = allCalls.FirstOrDefault();
        if (firstCall is not null)
        {
            RewriteFirstHopCall(firstCall);
            _firstHopDispatched = true;
        }

        // Rebuild updates: keep all non-tool-call content, keep only firstCall.
        var extraCalls = allCalls.Skip(1).ToHashSet();
        if (extraCalls.Count > 0)
        {
            var cleaned = new List<ChatResponseUpdate>();
            foreach (var update in updates)
            {
                var keptContents = update.Contents
                    .Where(c => c is not FunctionCallContent fc || !extraCalls.Contains(fc))
                    .ToList();
                var newUpdate = new ChatResponseUpdate
                {
                    Role = update.Role,
                    Contents = keptContents,
                    FinishReason = update.FinishReason,
                    ResponseId = update.ResponseId,
                    ModelId = update.ModelId,
                    CreatedAt = update.CreatedAt,
                    AdditionalProperties = update.AdditionalProperties,
                };
                cleaned.Add(newUpdate);
            }
            // Add correction hint to last update.
            cleaned.Last().Contents.Add(new TextContent(
                $"[SYSTEM] {extraCalls.Count} extra tool call(s) suppressed. Call ONE tool, then wait."));
            foreach (var u in cleaned) yield return u;
        }
        else
        {
            foreach (var update in updates) yield return update;
        }
    }

    // ── Core enforcement (non-streaming) ─────────────────────────────────────
    private ChatResponse EnforceAndRewrite(ChatResponse response)
    {
        // Find all tool calls across all assistant messages in this response.
        var allCalls = response.Messages
            .Where(m => m.Role == ChatRole.Assistant)
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();

        if (allCalls.Count == 0) return response;

        // Apply first-hop rewrite to the first call.
        if (!_firstHopDispatched)
        {
            RewriteFirstHopCall(allCalls[0]);
            _firstHopDispatched = true;
        }

        if (allCalls.Count <= 1) return response;

        // Multiple calls: rebuild messages keeping only the first tool call.
        // Build a set of the extra calls to remove.
        var extraCalls = allCalls.Skip(1).ToHashSet();
        var newMessages = new List<ChatMessage>();

        foreach (var msg in response.Messages)
        {
            if (msg.Role != ChatRole.Assistant)
            {
                newMessages.Add(msg);
                continue;
            }

            // Keep all non-tool-call content plus only the first call.
            var kept = msg.Contents
                .Where(c => c is not FunctionCallContent fc || !extraCalls.Contains(fc))
                .ToList();

            kept.Add(new TextContent(
                $"[SYSTEM] {extraCalls.Count} extra tool call(s) suppressed. Call ONE tool, then wait."));

            newMessages.Add(new ChatMessage(msg.Role, kept));
        }

        return new ChatResponse(newMessages)
        {
            FinishReason = response.FinishReason,
            ResponseId = response.ResponseId,
            ModelId = response.ModelId,
            CreatedAt = response.CreatedAt,
            Usage = response.Usage,
            AdditionalProperties = response.AdditionalProperties,
        };
    }

    // ── First-hop rewrite ─────────────────────────────────────────────────────
    private static void RewriteFirstHopCall(FunctionCallContent call)
    {
        if (!string.Equals(call.Name, "RouteToAgent", StringComparison.OrdinalIgnoreCase))
            return;
        if (call.Arguments is null) return;
        if (!call.Arguments.TryGetValue("agentName", out var agentNameObj)) return;
        var agentName = agentNameObj?.ToString() ?? string.Empty;
        if (!s_invalidFirstHops.Contains(agentName)) return;
        call.Arguments["agentName"] = "planner";
    }
}
