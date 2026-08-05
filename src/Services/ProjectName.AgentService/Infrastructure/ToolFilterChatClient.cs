// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Infrastructure/ToolFilterChatClient.cs
// Identity   : DelegatingChatClient that strips forbidden tools before every call
// Law Anchor : ARCH-NEW-001 (TYPE 1/TYPE 2 segregation)
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;

namespace ProjectName.AgentService.Infrastructure;

/// <summary>
/// Wraps an inner IChatClient and removes any tools whose names appear in
/// <paramref name="forbiddenTools"/> before forwarding requests.
/// Used to enforce the TYPE 1/TYPE 2 boundary and skill-load guards per ARCH-NEW-001.
/// </summary>
internal sealed class ToolFilterChatClient(IChatClient inner, string[] forbiddenTools)
    : DelegatingChatClient(inner)
{
    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => base.GetResponseAsync(messages, Filter(options), cancellationToken);

    public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => base.GetStreamingResponseAsync(messages, Filter(options), cancellationToken);

    private ChatOptions? Filter(ChatOptions? options)
    {
        if (options?.Tools is null || forbiddenTools.Length == 0) return options;

        var filtered = options.Tools
            .Where(t => t is not AIFunction f || !forbiddenTools.Contains(f.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (filtered.Count == options.Tools.Count) return options;

        var clone = options.Clone();
        clone.Tools = filtered;
        return clone;
    }
}
