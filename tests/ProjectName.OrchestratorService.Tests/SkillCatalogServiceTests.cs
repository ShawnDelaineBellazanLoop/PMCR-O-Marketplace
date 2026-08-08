using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjectName.OrchestratorService.Configuration;
using ProjectName.OrchestratorService.Skills;
using Xunit;

namespace ProjectName.OrchestratorService.Tests;

public sealed class SkillCatalogServiceTests
{
    [Fact]
    public async Task Search_returns_unique_declared_skill_names_from_staging()
    {
        var root = CreateTempDirectory();
        try
        {
            WriteSkill(root, "pmcro-engine", "planner", "Plan work");
            WriteSkill(root, "pmcro-specialty", "planner", "Duplicate plan name");
            WriteSkill(root, "dotnet", "dotnet-webapi", "Build APIs");

            var materializer = CreateMaterializer(root);
            await materializer.MaterializeAsync(TestContext.Current.CancellationToken);
            var catalog = new SkillCatalogService(
                NullLogger<SkillCatalogService>.Instance,
                Options.Create(new OrchestratorConfig
                {
                    FileSystemRoot = root,
                    MarketplaceRelativePath = ".agents/plugins/marketplace.json",
                    SkillsStagingPath = ".pmcro/skills-staging",
                }));

            var entries = catalog.Search();

            Assert.Equal(2, entries.Count);
            Assert.Equal(new[] { "dotnet-webapi", "planner" }, entries.Select(entry => entry.Name));
        }
        finally
        {
            DeleteTempDirectory(root);
        }
    }

    [Fact]
    public async Task Search_query_filters_by_skill_name_or_description()
    {
        var root = CreateTempDirectory();
        try
        {
            WriteSkill(root, "dotnet", "dotnet-webapi", "Build HTTP APIs");
            WriteSkill(root, "dotnet", "maui-theming", "Style mobile applications");

            var materializer = CreateMaterializer(root);
            await materializer.MaterializeAsync(TestContext.Current.CancellationToken);
            var catalog = new SkillCatalogService(
                NullLogger<SkillCatalogService>.Instance,
                Options.Create(new OrchestratorConfig
                {
                    FileSystemRoot = root,
                    MarketplaceRelativePath = ".agents/plugins/marketplace.json",
                    SkillsStagingPath = ".pmcro/skills-staging",
                }));

            var entries = catalog.Search("mobile");

            var entry = Assert.Single(entries);
            Assert.Equal("maui-theming", entry.Name);
        }
        finally
        {
            DeleteTempDirectory(root);
        }
    }

    private static void WriteSkill(string root, string plugin, string name, string description)
    {
        var skillRoot = Path.Combine(root, "plugins", plugin, "skills", name);
        Directory.CreateDirectory(skillRoot);
        File.WriteAllText(
            Path.Combine(skillRoot, "SKILL.md"),
            $"---\nname: {name}\ndescription: {description}\n---\n\n# {name}\n");
    }

    private static MarketplaceSkillsMaterializer CreateMaterializer(string root)
    {
        Directory.CreateDirectory(Path.Combine(root, ".agents", "plugins"));
        var plugins = Directory.GetDirectories(Path.Combine(root, "plugins"))
            .Select(path => new
            {
                name = Path.GetFileName(path),
                source = $"./plugins/{Path.GetFileName(path)}",
            });
        File.WriteAllText(
            Path.Combine(root, ".agents", "plugins", "marketplace.json"),
            System.Text.Json.JsonSerializer.Serialize(new { plugins }));

        return new MarketplaceSkillsMaterializer(
            NullLogger<MarketplaceSkillsMaterializer>.Instance,
            Options.Create(new OrchestratorConfig
            {
                FileSystemRoot = root,
                MarketplaceRelativePath = ".agents/plugins/marketplace.json",
                SkillsStagingPath = ".pmcro/skills-staging",
            }));
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "pmcro-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTempDirectory(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
    }
}
