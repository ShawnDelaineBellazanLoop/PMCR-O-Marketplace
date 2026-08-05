// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — ServiceDefaults
// File       : OllamaExtensions.cs
// Identity   : Keyed IChatClient factory for all Ollama model roles
// Law Anchor : OLS-001 (never use Microsoft.Extensions.AI.Ollama — use OllamaSharp)
//              FRAC-OLLAMA-TIMEOUT-001 (always supply infinite-timeout HttpClient)
// ThoughtLock: 2026-05-30
//
// FRAC-OLLAMA-TIMEOUT-001:
//   new OllamaApiClient(Uri, string) creates an internal HttpClient with Timeout=100s.
//   Any model that takes >100s to cold-load from VRAM → TaskCanceledException.
//   Fix: always use new OllamaApiClient(HttpClient) with Timeout.InfiniteTimeSpan.
//   Real timeout enforcement lives in the caller's CancellationToken, not HttpClient.
//
// Connection string resolution (three formats Aspire may inject):
//   A) Plain URI:          "http://ollama-server:11434"
//   B) URI + model hint:   "http://ollama-server:11434;model=qwen3:8b"
//   C) Key=Value (Aspire): "Endpoint=http://ollama-server:11434"
//   All three are handled by ParseEndpoint(). Model tag always comes from env var
//   or config key — never from the connection string model hint.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace ProjectName.ServiceDefaults;

public static class OllamaExtensions
{
    // ── Model role keys ───────────────────────────────────────────────────────
    // These match the Aspire resource names declared in AppHost.cs (AddModel calls).
    // ConnectionStrings__{key} is injected by Aspire via WithReference(modelResource).
    public static class Keys
    {
        public const string Default      = "model-default";
        public const string Orchestrator = "model-orchestrator";
        public const string Research     = "model-research";
        public const string Reflector    = "model-reflector";
        public const string Validator    = "model-validator";
        public const string Audit        = "model-audit";
        public const string Reactive     = "model-reactive";
        public const string Vision       = "model-vision";
    }

    // ── Registration entry point ──────────────────────────────────────────────
    // Call builder.AddOllamaClients() in each service's Program.cs.
    // Each role gets its own OllamaApiClient instance (separate HTTP connection)
    // so role-specific model tags and connection limits don't bleed across agents.
    public static TBuilder AddOllamaClients<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // All cognitive roles — env vars set by AppHost via WithEnvironment().
        // Fallback config keys used when running outside Aspire (dotnet run directly).
        builder.RegisterOllamaClient(
            Keys.Orchestrator,
            connectionStringKey: Keys.Orchestrator,
            modelEnvVar:  "OLLAMA_MODEL_ORCHESTRATOR",
            modelConfigKey: "Ollama:Models:Orchestrator");

        builder.RegisterOllamaClient(
            Keys.Research,
            connectionStringKey: Keys.Research,
            modelEnvVar:  "OLLAMA_MODEL_RESEARCH",
            modelConfigKey: "Ollama:Models:Research");

        builder.RegisterOllamaClient(
            Keys.Reflector,
            connectionStringKey: Keys.Reflector,
            modelEnvVar:  "OLLAMA_MODEL_REFLECTOR",
            modelConfigKey: "Ollama:Models:Reflector");

        builder.RegisterOllamaClient(
            Keys.Validator,
            connectionStringKey: Keys.Validator,
            modelEnvVar:  "OLLAMA_MODEL_VALIDATOR",
            modelConfigKey: "Ollama:Models:Validator");

        builder.RegisterOllamaClient(
            Keys.Audit,
            connectionStringKey: Keys.Audit,
            modelEnvVar:  "OLLAMA_MODEL_AUDIT",
            modelConfigKey: "Ollama:Models:Audit");

        builder.RegisterOllamaClient(
            Keys.Reactive,
            connectionStringKey: Keys.Reactive,
            modelEnvVar:  "OLLAMA_MODEL_REACTIVE",
            modelConfigKey: "Ollama:Models:Reactive");

        // Vision: multimodal model (llava:13b). Shares the same Ollama server
        // endpoint as the reactive model — different model tag only.
        builder.RegisterOllamaClient(
            Keys.Vision,
            connectionStringKey: Keys.Reactive,   // same server, different model tag
            modelEnvVar:  "OLLAMA_MODEL_VISION",
            modelConfigKey: "Ollama:Models:Vision");

        // Default: used by any service that just needs "a model" without a specific role.
        builder.RegisterOllamaClient(
            Keys.Default,
            connectionStringKey: Keys.Orchestrator, // same endpoint as orchestrator
            modelEnvVar:  "OLLAMA_MODEL_DEFAULT",
            modelConfigKey: "Ollama:Models:Default");

        return builder;
    }

    // ── Per-client factory ────────────────────────────────────────────────────
    // FRAC-OLLAMA-TIMEOUT-001: Never call new OllamaApiClient(uri, model).
    // That convenience overload creates a default HttpClient with Timeout=100s.
    private static void RegisterOllamaClient<TBuilder>(
        this TBuilder builder,
        string serviceKey,
        string connectionStringKey,
        string modelEnvVar,
        string modelConfigKey)
        where TBuilder : IHostApplicationBuilder
    {
        var config = builder.Configuration;

        builder.Services.AddKeyedSingleton<IChatClient>(serviceKey, (_, _) =>
        {
            var endpoint = ParseEndpoint(
                config.GetConnectionString(connectionStringKey),
                connectionStringKey);

            var modelTag =
                config[modelEnvVar]
                ?? config[modelConfigKey]
                ?? config["Ollama:Models:Default"]
                ?? "qwen3:8b";

            // FRAC-OLLAMA-TIMEOUT-001 fix: supply HttpClient with infinite timeout.
            var httpClient = new HttpClient
            {
                BaseAddress = endpoint,
                Timeout     = System.Threading.Timeout.InfiniteTimeSpan,
            };

            return new OllamaApiClient(httpClient) { SelectedModel = modelTag };
        });
    }

    // ── Connection string parser ──────────────────────────────────────────────
    // Handles plain URI, URI+model, and Aspire Endpoint=... formats.
    private static Uri ParseEndpoint(string? connectionString, string key)
    {
        if (string.IsNullOrEmpty(connectionString))
            return new Uri("http://localhost:11434");

        var segments = connectionString.Split(';',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        string? endpointRaw = null;

        foreach (var segment in segments)
        {
            var eqIdx = segment.IndexOf('=');
            if (eqIdx > 0)
            {
                var segKey = segment[..eqIdx].Trim();
                var segVal = segment[(eqIdx + 1)..].Trim();
                if (segKey.Equals("Endpoint", StringComparison.OrdinalIgnoreCase))
                {
                    endpointRaw = segVal;
                    break;
                }
            }
            else if (endpointRaw is null)
            {
                // First non-key=value segment is treated as the raw URI
                endpointRaw = segment;
            }
        }

        endpointRaw ??= segments[0];

        return Uri.TryCreate(endpointRaw, UriKind.Absolute, out var uri)
            ? uri
            : throw new InvalidOperationException(
                $"Invalid Ollama endpoint in ConnectionStrings__{key}: '{connectionString}' " +
                $"(resolved endpoint segment: '{endpointRaw}'). " +
                $"Expected a plain URI (e.g. 'http://localhost:11434') or " +
                $"a key=value string containing 'Endpoint=http://...'.");
    }
}
