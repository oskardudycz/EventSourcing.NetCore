using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

namespace IntroductionToEventSourcing.Tools;

public static class DockerCompose
{
    public const string Project = "eventsourcing";

    public static async Task<ushort?> PortAsync(string service, ushort containerPort)
    {
        using var client = TestcontainersSettings.OS.DockerEndpointAuthConfig
            .GetDockerClientBuilder(Guid.NewGuid())
            .Build();

        var containers = await client.Containers.ListContainersAsync(
            new ContainersListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    ["label"] = new Dictionary<string, bool>
                    {
                        [$"com.docker.compose.project={Project}"] = true,
                        [$"com.docker.compose.service={service}"] = true
                    }
                }
            }
        ).ConfigureAwait(false);

        return containers
            .SelectMany(container => container.Ports)
            .Where(port => port.PrivatePort == containerPort && port.PublicPort != 0)
            .Select(port => (ushort?)port.PublicPort)
            .FirstOrDefault();
    }
}
