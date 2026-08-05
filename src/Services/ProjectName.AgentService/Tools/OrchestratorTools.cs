// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Tools/OrchestratorTools.cs
// Identity   : RouteToAgent + GetAgentCapabilities — orchestrator tool set
// Law Anchor : FRAC-ROUTING-PARALLEL-001, FRAC-SELF-URL-ASPIRE-001
// ThoughtLock: 2026-05-31
//
// FRAC-DEAD-AGENTS-001 (resolved 2026-05-31):
//   researcher and auditor were listed in GetAgentCapabilities() but never
//   registered in Program.cs. The orchestrator's model could call
//   RouteToAgent("researcher", ...) → HTTP call to /researcher/v1/responses → 404/hang.
//   Resolution: removed from capabilities list. Re-add only when fully registered.
// ═══════════════════════════════════════════════════════════════════════════════

using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectName.AgentService.Tools;

/// <summary>
/// I AM the orchestration tool set — routing and capability discovery.
/// RouteToAgent is the primary tool for the PMCRO phase loop.
/// </summary>
public static class OrchestratorTools
{
    private static readonly Lazy<HttpClient> _self = new(() =>
        new HttpClient
        {
            BaseAddress = new Uri(ResolveSelfUrl()),
            Timeout     = TimeSpan.FromMinutes(5),
        });

    private static string ResolveSelfUrl()
    {
        var explicit_ = Environment.GetEnvironmentVariable("AGENTSERVICE_SELF_URL");
        if (!string.IsNullOrWhiteSpace(explicit_))
            return explicit_.TrimEnd('/');

        var aspnetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "";
        string? bestHttp  = null;
        string? bestHttps = null;

        foreach (var raw in aspnetUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = raw
                .Replace("://+:",       "://localhost:", StringComparison.OrdinalIgnoreCase)
                .Replace("://0.0.0.0:", "://localhost:", StringComparison.OrdinalIgnoreCase)
                .Replace("://[::]:",    "://localhost:", StringComparison.OrdinalIgnoreCase)
                .TrimEnd('/');

            if (normalized.StartsWith("http://",  StringComparison.OrdinalIgnoreCase)) bestHttp  ??= normalized;
            if (normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) bestHttps ??= normalized;
        }

        return bestHttp ?? bestHttps ?? "http://localhost:5000";
    }

    /// <summary>Public wrapper for boot diagnostics.</summary>
    public static string ResolveSelfUrlPublic() => ResolveSelfUrl();

    [Description(
        "Route a task to the next phase agent in the PMCRO loop. " +
        "STEP 1 IS ALWAYS 'planner' — for every task, no exceptions. " +
        "NEVER route to 'maker', 'checker', or 'reflector' as the first call. " +
        "Call this ONCE per phase. Do NOT retry. Wait for the response.")]
    public static async Task<string> RouteToAgent(
        [Description("Agent name. FOR THE FIRST CALL this MUST be 'planner'. Sequence: planner → maker → checker → reflector. Never skip planner.")] string agentName,
        [Description("The full seed intent as PLAIN TEXT. Example: 'Read A:\\PMCR-O\\README.md and summarise it.'")] string task)
    {
        try
        {
            var body = new
            {
                model = agentName,
                agent = new { name = agentName },
                input = task,
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));
            var response = await _self.Value.PostAsJsonAsync($"/{agentName}/v1/responses", body, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cts.Token);
                return $"RouteToAgent error [{agentName}] HTTP {(int)response.StatusCode}: {err[..Math.Min(500, err.Length)]}";
            }

            var rawJson = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in output.EnumerateArray())
                {
                    if (!item.TryGetProperty("content", out var content)) continue;
                    if (content.ValueKind != JsonValueKind.Array) continue;
                    foreach (var block in content.EnumerateArray())
                    {
                        if (block.TryGetProperty("type", out var ct) &&
                            ct.GetString() is "output_text" or "text" &&
                            block.TryGetProperty("text", out var text))
                            return text.GetString() ?? string.Empty;
                    }
                }
            }

            return rawJson;
        }
        catch (Exception ex)
        {
            return $"RouteToAgent failed [{agentName}]: {ex.GetType().Name}: {ex.Message}";
        }
    }

    [Description("Return the capability summary of all registered PMCRO agents.")]
    public static string GetAgentCapabilities()
        => """
           orchestrator : PMCRO hybrid loop controller + router
           planner      : PMCRO PLAN — deliberative plan producer
           maker        : PMCRO MAKE — reactive plan executor (all MCP tools)
           checker      : PMCRO CHECK — goal-oriented output scorer
           reflector    : PMCRO REFLECT — learning agent, issues verdict

           NOTE: researcher and auditor are PLANNED but not yet registered.
           Do NOT route to researcher or auditor — they will return errors.
           """;
}
