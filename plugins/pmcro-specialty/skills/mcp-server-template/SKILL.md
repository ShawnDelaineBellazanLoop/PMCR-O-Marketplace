---
name: mcp-server-template
description: "USE FOR: scaffolding a new subject-agent MCP server under W:\\PMCR-O\\mcp\\
  matching the exact structure already proven by ProjectName.Mcp.Filesystem,
  ProjectName.Mcp.Playwright, and ProjectName.Mcp.Terminal -- same folder layout,
  same Program.cs wiring, same three-pillar registration. Fill in the template's
  placeholders (service name, identity string, law anchor, pillar descriptions)
  for a new server; do not invent a different structure. DO NOT USE FOR: writing
  the actual Tools/Resources/Prompts logic itself -- this skill only produces the
  scaffold, not the domain behavior inside it."
metadata:
  pmcro_provides: "mcp-server-template"
  pmcro_requires: "none"
compatibility: ".NET 10, matches ProjectName.ServiceDefaults and the MCP SDK
  usage already in the three existing servers under mcp\\."
---

# MCP Server Template

Every existing subject-agent MCP server in this repo follows one identical
skeleton. This skill reproduces that skeleton exactly for a new server,
verified against `ProjectName.Mcp.Filesystem\Program.cs` line by line -- not
a generic MCP tutorial pattern.

## Verified folder layout (all three existing servers match this)

```
ProjectName.Mcp.<Name>/
  Configuration/    <Name>Config.cs      -- one singleton, enforces boundary law
  Tools/            <Name>Tools.cs       -- Pillar 1
  Resources/        <Name>Resources.cs   -- Pillar 2
  Prompts/          <Name>Prompts.cs     -- Pillar 3
  Program.cs
  ProjectName.Mcp.<Name>.csproj
  Dockerfile
  appsettings.json / appsettings.Development.json
```

## Program.cs template (verified pattern, placeholders only)

```csharp
using ProjectName.Mcp.<Name>.Configuration;
using ProjectName.Mcp.<Name>.Prompts;
using ProjectName.Mcp.<Name>.Resources;
using ProjectName.Mcp.<Name>.Tools;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// 1. INFRASTRUCTURE SINGLETON -- enforces this server's boundary law
builder.Services.AddSingleton<<Name>Config>();

// 2. MCP PILLAR SINGLETONS
builder.Services.AddSingleton<<Name>Tools>();
builder.Services.AddSingleton<<Name>Resources>();
builder.Services.AddSingleton<<Name>Prompts>();

// 3. MCP SERVER CONFIGURATION -- Stateless HTTP, matches all existing servers
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => { options.Stateless = true; })
    .WithTools<<Name>Tools>()
    .WithResources<<Name>Resources>()
    .WithPrompts<<Name>Prompts>();

var app = builder.Build();
app.MapDefaultEndpoints();
app.MapMcp("/mcp");

// 5. DIAGNOSTIC ROOT -- "identity" here is just this string, nothing else
app.MapGet("/", (<Name>Config config) => new
{
    service = "ProjectName.Mcp.<Name>",
    identity = "<one-line description of what this server actuates>",
    status = "Online",
    mcp_endpoint = "/mcp",
    pillars = new
    {
        pillar1_tools = "<what Tools does>",
        pillar2_resources = "<what Resources does>",
        pillar3_prompts = "<what Prompts does>"
    },
    compliance = new[] { "<LAW-ANCHOR-ID>: <what it prevents>" }
});

app.Run();
```

## Steps to scaffold a new server

1. Pick `<Name>` (e.g. `Wendy's` job data becomes `JobSearch`, not a
   platform's brand name -- keep the server generic to the capability, not
   the site).
2. Create the six subfolders + files above under `mcp\ProjectName.Mcp.<Name>\`.
3. Fill `Program.cs` placeholders only -- do not restructure the wiring
   order (Config singleton before pillar singletons before MCP server
   config is load-bearing, not stylistic, per FS-LAW-001's pattern).
4. Write a matching `skills\<name>-agent\SKILL.md` (Colony Laws section,
   TYPE1/TYPE2 discipline per tool) so subject-agent instructions exist --
   see `skills\playwright-agent\SKILL.md` as the reference for that half.
5. Register the new server the same way the existing three are registered
   (check `OrchestratorService` startup / `McpToolCache` wiring before
   assuming -- do not guess this step; it wasn't re-verified in this pass).

## What this skill does not do

- Does not write real Tool/Resource/Prompt logic -- that's domain work,
  different every time.
- Does not decide TYPE1 vs TYPE2 classification for new tools -- that's a
  judgment call per tool, made explicitly, not defaulted.
- Does not skip step 5's registration -- an unregistered server is a
  scaffold, not a working subject agent.
