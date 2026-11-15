using Core.Testing.Fixtures;
using DotNet.Testcontainers.Builders;
using Xunit;

namespace Core.EntityFramework.Tests.Fixtures;

public class DockerComposeTests
{
    [Fact]
    public async Task PortAsync_ReturnsPublishedPort_OfRunningComposeService()
    {
        var project = $"docker-compose-tests-{Guid.NewGuid():N}";
        await using var container = new ContainerBuilder("postgres:18.7-alpine")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithLabel("com.docker.compose.project", project)
            .WithLabel("com.docker.compose.service", "postgres")
            .WithPortBinding(5432, true)
            .Build();
        await container.StartAsync(TestContext.Current.CancellationToken);

        var port = await DockerCompose.PortAsync(project, "postgres", 5432, TestContext.Current.CancellationToken);

        Assert.Equal(container.GetMappedPublicPort(5432), port);
    }

    [Fact]
    public async Task PortAsync_ReturnsNull_WhenComposeServiceIsNotRunning()
    {
        var port = await DockerCompose.PortAsync(
            $"docker-compose-tests-{Guid.NewGuid():N}", "postgres", 5432, TestContext.Current.CancellationToken
        );

        Assert.Null(port);
    }
}
