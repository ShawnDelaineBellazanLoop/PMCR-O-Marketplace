using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjectName.OrchestratorService.Configuration;
using ProjectName.OrchestratorService.Skills;
using Xunit;

namespace ProjectName.OrchestratorService.Tests;

public sealed class MarketplaceSkillTests
{
    [Fact]
    public void CatalogService_UsesConfiguredMarketplaceRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "pmcro-tests", Guid.NewGuid().ToString("N"));
        var skillRoot = Path.Combine(root, "plugins", "sample", "skills", "planner");
        Directory.CreateDirectory(skillRoot);
        Directory.CreateDirectory(Path.Combine(root, ".agents", "plugins"));
        File.WriteAllText(Path.Combine(skillRoot, "SKILL.md"), "name: planner\ndescription: Plan work");
        File.WriteAllText(Path.Combine(root, ".agents", "plugins", "marketplace.json"), "{\"plugins\":[{\"name\":\"sample\",\"source\":\"./plugins/sample\"}]}");

        try
        {
            var catalog = new SkillCatalogService(
                NullLogger<SkillCatalogService>.Instance,
                Options.Create(new OrchestratorConfig
                {
                    FileSystemRoot = root,
                    MarketplaceRelativePath = ".agents/plugins/marketplace.json",
                }));

            var snapshot = catalog.GetSnapshot();
            Assert.Equal(1, snapshot.Count);
            Assert.Equal("planner", Assert.Single(snapshot.Skills).Name);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Materializer_ResolvesConfiguredStagingPathUnderRepositoryRoot()
    {
        var config = Options.Create(new OrchestratorConfig
        {
            FileSystemRoot = @"C:\repo",
            SkillsStagingPath = ".pmcro/skills-staging",
        });
        var materializer = new MarketplaceSkillsMaterializer(
            NullLogger<MarketplaceSkillsMaterializer>.Instance,
            config);

        var expected = Path.GetFullPath(Path.Combine(@"C:\repo", ".pmcro", "skills-staging"));
        Assert.Equal(expected, materializer.StagingRoot);
    }

    [Fact]
    public void Materializer_ResolvesAbsoluteStagingPathWithoutRebasing()
    {
        var absolute = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "pmcro-staging"));
        var config = Options.Create(new OrchestratorConfig
        {
            FileSystemRoot = @"C:\repo",
            SkillsStagingPath = absolute,
        });
        var materializer = new MarketplaceSkillsMaterializer(
            NullLogger<MarketplaceSkillsMaterializer>.Instance,
            config);

        Assert.Equal(absolute, materializer.StagingRoot);
    }

    [Fact]
    public void CatalogService_ReadsCanonicalMarketplaceAndDeduplicatesNames()
    {
        var root = Path.Combine(Path.GetTempPath(), "pmcro-tests", Guid.NewGuid().ToString("N"));
        var pluginA = Path.Combine(root, "plugins", "plugin-a", "skills", "shared");
        var pluginB = Path.Combine(root, "plugins", "plugin-b", "skills", "shared");
        Directory.CreateDirectory(pluginA);
        Directory.CreateDirectory(pluginB);
        Directory.CreateDirectory(Path.Combine(root, ".agents", "plugins"));
        File.WriteAllText(Path.Combine(pluginA, "SKILL.md"), "name: shared\ndescription: first");
        File.WriteAllText(Path.Combine(pluginB, "SKILL.md"), "name: shared\ndescription: second");
        File.WriteAllText(Path.Combine(root, ".agents", "plugins", "marketplace.json"),
            "{\"plugins\":[{\"name\":\"plugin-a\",\"source\":\"./plugins/plugin-a\"},{\"name\":\"plugin-b\",\"source\":\"./plugins/plugin-b\"}]}");

        try
        {
            var materializer = new MarketplaceSkillsMaterializer(
                NullLogger<MarketplaceSkillsMaterializer>.Instance,
                Options.Create(new OrchestratorConfig
                {
                    FileSystemRoot = root,
                    MarketplaceRelativePath = ".agents/plugins/marketplace.json",
                    SkillsStagingPath = ".pmcro/skills-staging",
                }));
            var catalog = new SkillCatalogService(
                NullLogger<SkillCatalogService>.Instance,
                Options.Create(new OrchestratorConfig
                {
                    FileSystemRoot = root,
                    MarketplaceRelativePath = ".agents/plugins/marketplace.json",
                    SkillsStagingPath = ".pmcro/skills-staging",
                }));

            var snapshot = catalog.GetSnapshot();

            Assert.Equal(1, snapshot.Count);
            var shared = Assert.Single(snapshot.Skills, entry => entry.Name == "shared");
            Assert.Equal("plugin-a", shared.Plugin);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
