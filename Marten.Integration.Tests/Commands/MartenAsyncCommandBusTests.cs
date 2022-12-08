using Core;
using Core.Commands;
using Core.Events;
using Core.Marten;
using Core.Marten.Commands;
using Core.OpenTelemetry;
using Core.Testing;
using FluentAssertions;
using Marten.Events.Daemon;
using Marten.Integration.Tests.TestsInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Polly;
using Xunit;

namespace Marten.Integration.Tests.Commands;

public class MartenAsyncCommandBusTests: MartenTest
{
    private readonly MartenAsyncCommandBus martenAsyncCommandBus;
    private readonly List<Guid> userIds = new();
    private readonly EventListener eventListener = new();
    private readonly AsyncProjectionHostedService asyncDaemon;
    private readonly CancellationToken ct = new CancellationTokenSource().Token;

    public MartenAsyncCommandBusTests(): base(false)
    {
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
                ConnectionString = Settings.ConnectionString,
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
        Session = serviceProvider.GetRequiredService<IDocumentSession>();

        asyncDaemon = (AsyncProjectionHostedService)serviceProvider.GetRequiredService<IHostedService>();

        martenAsyncCommandBus = new MartenAsyncCommandBus(Session);
    }

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
