# PMCR-O-Marketplace

PMCR-O Colony - Agent Skills Marketplace. Curated Plan/Make/Check/Reflect/Orchestrate
(PMCR-O) Colony skills, C-Suite domain skills, and .NET agent skills, packaged for
Claude Code, Cursor, VS Code, and Codex CLI.

## What's Included

| Plugin | Description |
| --- | --- |
| pmcro-engine | PMCR-O core engine: Plan, Make, Check, Reflect orchestration and loop management |
| pmcro-csuite | C-Suite executive domain skills: CEO, CFO, CTO, COO, CMO, CRO, CHRO, CLO, Chief of Staff |
| pmcro-specialty | Specialty skills: powershell-expert, property-preservation, career-evidence, skill-creator |
| dotnet | C# language server (LSP) integration for coding agents and high-level .NET development skills. |
| dotnet-advanced | Advanced .NET and C# skills for niche scenarios that are not part of the core dotnet plugin. |
| dotnet-data | Skills for .NET data access and Entity Framework related tasks. |
| dotnet-diag | Skills for .NET performance investigations, debugging, and incident analysis. |
| dotnet-msbuild | Comprehensive MSBuild and .NET build skills: failure diagnosis, performance optimization, code quality, and modernization. |
| dotnet-nuget | NuGet and .NET package management: dependency management and modernization. |
| dotnet-upgrade | Skills for migrating and upgrading .NET projects across framework versions, language features, and compatibility targets. |
| dotnet-maui | Skills for .NET MAUI development: environment setup, diagnostics, troubleshooting, navigation, data binding, dependency injection, layout, and theming. |
| dotnet-ai | AI and ML skills for .NET: technology selection, LLM integration, agentic workflows, RAG pipelines, MCP, and classic ML with ML.NET. |
| dotnet-template-engine | .NET Template Engine skills: template discovery, project scaffolding, and template authoring. |
| dotnet-test | Skills for running, generating, analyzing, and improving .NET tests: test execution, filtering, platform detection, coverage, testability, and MSTest workflows. |
| dotnet-test-migration | Skills and an orchestrator agent for migrating .NET test frameworks and platforms: MSTest and xUnit version upgrades, xUnit-to-MSTest conversion, and VSTest to Microsoft.Testing.Platform. |
| dotnet-aspnetcore | ASP.NET Core web development skills including middleware, endpoints, real-time communication, and API patterns. |
| dotnet-blazor | Skills for Blazor development: component authoring, interactivity, and web application patterns. |
| dotnet11 | Skills for new .NET 11 APIs and language features. |
| dotnet-experimental | Experimental skills under active evaluation that may change or graduate to stable plugins. |

## Installation

### Plugins - Copilot CLI / Claude Code

1. Launch Copilot CLI or Claude Code
2. Add the marketplace: `/plugin marketplace add <this-repo>`
3. Install a plugin: `/plugin install <plugin>@pmcro-colony`
4. Restart to load the new plugins
5. View available skills: `/skills`
6. View available agents: `/agents`
7. Update plugin (on demand): `/plugin update <plugin>@pmcro-colony`

### VS Code / VS Code Insiders (Preview)

> VS Code plugin support is a preview feature and subject to change. You may need to
> enable it first.

```json
// settings.json
{
  "chat.plugins.enabled": true,
  "chat.plugins.marketplaces": ["<this-repo>"]
}
```

Once configured, type `/plugins` in Copilot Chat or use the `@agentPlugins` filter in
Extensions to browse and install plugins from the marketplace.

### Cursor

This repository is a Cursor plugin marketplace. You can discover and install
published plugins directly in Cursor:

1. Open the marketplace panel in Cursor
2. Search for pmcro or dotnet, or browse the marketplace catalog
3. Install the desired plugins

For local development or unpublished changes, import plugins from a local checkout:

- Copy or symlink your local checkout to `~/.cursor/plugins/local/pmcro-colony`
- Restart Cursor or run `Developer: Reload Window`

### Codex CLI

Skills in this repository follow the agentskills.io open standard and are
compatible with OpenAI Codex.

#### Plugin marketplace (recommended)

This repository ships a Codex-native marketplace manifest at
`.agents/plugins/marketplace.json`, so you can register it as a marketplace and
install plugins from it directly.

1. Add the marketplace: `codex plugin marketplace add <this-repo>`
2. Launch Codex and open the plugin browser: `/plugins`
3. Browse the `pmcro-colony` tab and install the desired plugins
4. Update plugins on demand: `codex plugin marketplace upgrade pmcro-colony`

#### Individual skills

You can also install individual skills using the skill-installer CLI with the
GitHub URL:

```sh
$ skill-installer install https://github.com/<owner>/<repo>/tree/main/plugins/<plugin>/skills/<skill-name>
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines and how to add
a new plugin.

## License

See [LICENSE](LICENSE) for details.
