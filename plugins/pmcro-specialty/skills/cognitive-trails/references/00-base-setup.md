# Reference: 00 — Zero to Agent
# Level 0 — From dotnet new to Your First Running Agent

Validated versions (2026-05-29):
- .NET 10 LTS
- MAF 1.7.0 (Microsoft.Agents.AI)
- MCP 1.3.0 (ModelContextProtocol, ModelContextProtocol.AspNetCore)
- OllamaSharp 5.4.25 (local inference — qwen3:8b)
- .NET Aspire 13.3.4 (optional at this level, required at Level 3+)

---

## Stage 0-A: Prerequisites

```bash
# 1. Install .NET 10 SDK
dotnet --version   # must be 10.x

# 2. Install Ollama (local LLM — free, no API key)
# https://ollama.com/download
ollama pull qwen3:8b   # ~5 GB
```

---

## Stage 0-B: The Simplest Possible Agent

```bash
mkdir CognitiveTrails.Hello && cd CognitiveTrails.Hello
dotnet new console
dotnet add package Microsoft.Agents.AI --version 1.7.0
dotnet add package Microsoft.Agents.AI.Ollama --version 1.7.0
```

```csharp
// Program.cs
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Ollama;

var chatClient = new OllamaChatClient(
    new Uri("http://localhost:11434"),
    "qwen3:8b");

// "I Am" framing — identity declared, not assigned
var agent = new AIAgent(
    chatClient,
    instructions: """
        I Am a helpful cognitive assistant.
        I answer questions clearly and concisely.
        I never fabricate. Null over hallucination.
        """);

var response = await agent.RunAsync("What is the capital of France?");
Console.WriteLine($"Agent: {response}");
```

```bash
dotnet run
# Agent: The capital of France is Paris.
```

**That's it. A running agent. Everything else is earned, not assumed.**

---

## Stage 0-C: Add an MCP Tool Server

```bash
mkdir CognitiveTrails.Mcp.Filesystem && cd CognitiveTrails.Mcp.Filesystem
dotnet new web
dotnet add package ModelContextProtocol.AspNetCore --version 1.3.0
```

```csharp
// Program.cs — Minimal MCP server
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithTools<FileSystemTools>();

var app = builder.Build();
app.MapMcp();
await app.RunAsync();
```

```csharp
// FileSystemTools.cs
using ModelContextProtocol;

[McpServerToolType]
public class FileSystemTools
{
    [McpServerTool(Name = "read_file", Description = "Read a file at the given path.")]
    public static async Task<string> ReadFileAsync(
        [McpServerToolParameter(Description = "File path.")] string path,
        CancellationToken ct = default)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            return $"{{\"error\": \"File not found: {path}\"}}";
        return await File.ReadAllTextAsync(fullPath, ct);
    }
}
```

```bash
# Run MCP server on port 5100
dotnet run --urls "http://localhost:5100"
```

### Wire MCP to Agent

```csharp
using Microsoft.Agents.AI.Mcp;

var mcpClient = new HttpMcpClient(new Uri("http://localhost:5100"));
var tools = await mcpClient.GetToolsAsync();

var agent = new AIAgent(
    chatClient,
    instructions: """
        I Am a cognitive assistant with filesystem access.
        I use read_file to read files when asked. I never guess file contents.
        """,
    tools: tools);

var response = await agent.RunAsync("Read ./README.md and summarize it.");
Console.WriteLine(response);
```

---

## Stage 0-D: What You Have

```
CognitiveTrails.Hello/            ← Agent (the brain)
CognitiveTrails.Mcp.Filesystem/   ← MCP Server (the hands)
```

Augmented LLM — Anthropic's base unit:
- LLM ✓
- Tools via MCP ✓
- Identity (I Am framing) ✓

**Next:** Add multiple agents in a workflow.
→ See `01-maf-architecture.md`

---

## Version Lock

```xml
<PackageVersion Include="Microsoft.Agents.AI"              Version="1.7.0" />
<PackageVersion Include="Microsoft.Agents.AI.Ollama"       Version="1.7.0" />
<PackageVersion Include="ModelContextProtocol"             Version="1.3.0" />
<PackageVersion Include="ModelContextProtocol.AspNetCore"  Version="1.3.0" />
<PackageVersion Include="Microsoft.Extensions.AI"         Version="10.6.0" />
```
