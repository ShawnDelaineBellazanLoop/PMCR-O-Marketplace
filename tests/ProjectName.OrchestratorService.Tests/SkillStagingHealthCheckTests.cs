using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjectName.OrchestratorService.Configuration;
using ProjectName.OrchestratorService.Skills;
using Xunit;

namespace ProjectName.OrchestratorService.Tests;

public sealed class SkillStagingHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_returns_healthy_when_skill_manifest_is_staged()
    {
        var root = Path.Combine(Path.GetTempPath(), "pmcro-health", Guid.NewGuid().ToString("N"));
        var staging = Path.Combine(root, ".pmcro", "skills-staging", "plugin", "skill");
        Directory.CreateDirectory(staging);
        await File.WriteAllTextAsync(
            Path.Combine(staging, "SKILL.md"),
            "name: skill\ndescription: test\n",
            TestContext.Current.CancellationToken);

        try
        {
            var materializer = new MarketplaceSkillsMaterializer(
                NullLogger<MarketplaceSkillsMaterializer>.Instance,
                Options.Create(new OrchestratorConfig
                {
                    FileSystemRoot = root,
                    SkillsStagingPath = ".pmcro/skills-staging",
                }));
            var check = new SkillStagingHealthCheck(materializer);

            var result = await check.CheckHealthAsync(
                new HealthCheckContext(),
                TestContext.Current.CancellationToken);

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CheckHealthAsync_returns_unhealthy_when_staging_is_empty()
    {
        var root = Path.Combine(Path.GetTempPath(), "pmcro-health", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var materializer = new MarketplaceSkillsMaterializer(
                NullLogger<MarketplaceSkillsMaterializer>.Instance,
                Options.Create(new OrchestratorConfig
                {
                    FileSystemRoot = root,
                    SkillsStagingPath = ".pmcro/missing",
                }));
            var check = new SkillStagingHealthCheck(materializer);

            var result = await check.CheckHealthAsync(
                new HealthCheckContext(),
                TestContext.Current.CancellationToken);

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
