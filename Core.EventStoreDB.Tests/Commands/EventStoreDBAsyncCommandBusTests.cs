using Core;
using Core.Commands;
using Core.Events;
using Core.EventStoreDB;
using Core.EventStoreDB.Commands;
using Core.EventStoreDB.Events;
using Core.OpenTelemetry;
using Core.Testing;
using EventStore.Client;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Polly;
using Xunit;

namespace EventStoreDB.Integration.Tests.Commands;

public class EventStoreDBAsyncCommandBusTests
{
    private readonly EventStoreDBAsyncCommandBus martenAsyncCommandBus;
    private readonly List<Guid> userIds = new();
    private readonly EventListener eventListener = new();
    private readonly CancellationToken ct = new CancellationTokenSource().Token;
    private readonly EventStoreClient eventStoreClient;
    private readonly IHostedService subscriptionToAll;

    public EventStoreDBAsyncCommandBusTests()
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
            .AddEventStoreDB(new EventStoreDBConfig
            {
                ConnectionString = "esdb://localhost:2113?tls=false"
            })
            .AddEventStoreDBSubscriptionToAll()
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
        eventStoreClient = serviceProvider.GetRequiredService<EventStoreClient>();

        subscriptionToAll = serviceProvider.GetRequiredService<IHostedService>();

        martenAsyncCommandBus = new EventStoreDBAsyncCommandBus(eventStoreClient);
    }

    [Fact]
    public async Task CommandIsStoredInEventStoreDBAndForwardedToCommandHandler()
    {
        // Given
        var userId = Guid.NewGuid();
        var command = new AddUser(userId);

        // When
        await subscriptionToAll.StartAsync(ct);

        await martenAsyncCommandBus.Schedule(command, ct);

        // Then
        await eventListener.WaitForProcessing(command, ct);

        var commands = await eventStoreClient.ReadStream(EventStoreDBAsyncCommandBus.CommandsStreamId, ct);
        commands.Should().HaveCountGreaterThanOrEqualTo(1);
        commands.OfType<AddUser>()
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

    public Task Handle(AddUser command, CancellationToken cancellationToken)
    {
        userIds.Add(command.UserId);

        return Task.CompletedTask;
    }
}
