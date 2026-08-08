using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjectName.OrchestratorService.Configuration;
using ProjectName.OrchestratorService.Skills;
using Xunit;

namespace ProjectName.OrchestratorService.Tests;

public sealed class MarketplaceSkillsMaterializerTests
{
    [Fact]
    public async Task MaterializeAsync_copies_registered_skill_into_configured_staging_root()
    {
        var root = CreateTempDirectory();
        try
        {
            var pluginRoot = Path.Combine(root, "plugins", "sample-plugin");
            var skillRoot = Path.Combine(pluginRoot, "skills", "sample-skill");
            Directory.CreateDirectory(skillRoot);
            await File.WriteAllTextAsync(
                Path.Combine(skillRoot, "SKILL.md"),
                "---\nname: sample-skill\ndescription: Sample skill\n---\n\n# Sample\n",
                TestContext.Current.CancellationToken);
            Directory.CreateDirectory(Path.Combine(root, ".agents", "plugins"));
            await File.WriteAllTextAsync(
                Path.Combine(root, ".agents", "plugins", "marketplace.json"),
                "{\"plugins\":[{\"name\":\"sample-plugin\",\"source\":\"./plugins/sample-plugin\"}]}",
                TestContext.Current.CancellationToken);

            var materializer = CreateMaterializer(root);
            var count = await materializer.MaterializeAsync(TestContext.Current.CancellationToken);

            Assert.Equal(1, count);
            Assert.True(File.Exists(Path.Combine(
                root, ".pmcro", "skills-staging", "sample-plugin", "sample-skill", "SKILL.md")));
        }
        finally
        {
            DeleteTempDirectory(root);
        }
    }

    private static MarketplaceSkillsMaterializer CreateMaterializer(string root)
    {
        var config = Options.Create(new OrchestratorConfig
        {
            FileSystemRoot = root,
            MarketplaceRelativePath = ".agents/plugins/marketplace.json",
            SkillsStagingPath = ".pmcro/skills-staging",
        });
        return new MarketplaceSkillsMaterializer(
            NullLogger<MarketplaceSkillsMaterializer>.Instance,
            config);
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
