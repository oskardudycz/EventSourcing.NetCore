using Core;
using Core.Commands;
using Core.Events;
using Core.Marten;
using Core.Marten.Commands;
using Core.OpenTelemetry;
using Core.Testing;
using FluentAssertions;
using Marten.Events.Daemon;
using Marten.Events.Daemon.Coordination;
using Marten.Integration.Tests.TestsInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Polly;
using Xunit;

namespace Marten.Integration.Tests.Commands;

public class MartenAsyncCommandBusTests(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer, true)
{
    [Fact]
    public async Task CommandIsStoredInMartenAndForwardedToCommandHandler()
    {
        // Given
        var userId = Guid.NewGuid();
        var command = new AddUser(userId);

        // When
        await asyncDaemon.StartAsync(ct);

        await martenAsyncCommandBus.Schedule(command, ct);

        // Then
        await eventListener.WaitForProcessing(command, ct);

        var commands = await Session.Events.FetchStreamAsync(MartenAsyncCommandBus.CommandsStreamId, token: ct);
        commands.Should().HaveCountGreaterThanOrEqualTo(1);
        commands.Select(e => e.Data).OfType<AddUser>()
            .Count(e => e.UserId == userId)
            .Should().Be(1);

        userIds.Should().Contain(userId);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var services = new ServiceCollection();

        services
            .AddLogging()
            .AddSingleton<IHostEnvironment, HostingEnvironment>(
                _ => new HostingEnvironment { EnvironmentName = Environments.Development }
            )
            .AddSingleton<IEventBus>(sp =>
                new EventCatcher(
                    eventListener,
                    sp.GetRequiredService<EventBus>()
                )
            )
            .AddCoreServices()
            .AddMarten(new MartenConfig
            {
                ConnectionString = ConnectionString,
                WriteModelSchema = SchemaName,
                ReadModelSchema = SchemaName
            })
            .AddCommandHandler<AddUser, AddUserCommandHandler>(
                _ => new AddUserCommandHandler(userIds)
            )
            .AddScoped(sp => new InMemoryCommandBus(
                sp,
                new ActivityScope(),
                Policy.NoOpAsync()
            ))
            .AddSingleton(eventListener)
            .AddCommandForwarder();

        var serviceProvider = services.BuildServiceProvider();
        var session = serviceProvider.GetRequiredService<IDocumentSession>();

        asyncDaemon = serviceProvider.GetRequiredService<IProjectionCoordinator>();

        martenAsyncCommandBus = new MartenAsyncCommandBus(session);
    }

    private MartenAsyncCommandBus martenAsyncCommandBus = default!;
    private readonly List<Guid> userIds = [];
    private readonly EventListener eventListener = new();
    private IProjectionCoordinator asyncDaemon = default!;
    private readonly CancellationToken ct = new CancellationTokenSource().Token;
}

public record AddUser(Guid UserId, string? Sth = default);

internal class AddUserCommandHandler: ICommandHandler<AddUser>
{
    private readonly List<Guid> userIds;

    public AddUserCommandHandler(List<Guid> userIds) =>
        this.userIds = userIds;

    public Task Handle(AddUser command, CancellationToken ct)
    {
        userIds.Add(command.UserId);

        return Task.CompletedTask;
    }
}
