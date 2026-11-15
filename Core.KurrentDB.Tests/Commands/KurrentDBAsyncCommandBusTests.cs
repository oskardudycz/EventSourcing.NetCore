using System.Diagnostics.Metrics;
using Core;
using Core.Commands;
using Core.Events;
using Core.KurrentDB;
using Core.KurrentDB.Commands;
using Core.KurrentDB.Events;
using Core.KurrentDB.Subscriptions;
using Core.OpenTelemetry;
using Core.Testing;
using Core.Testing.Fixtures;
using KurrentDB.Client;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Time.Testing;
using Polly;
using Xunit;

namespace KurrentDB.Integration.Tests.Commands;

public class KurrentDBAsyncCommandBusTests
{
    private readonly KurrentDBAsyncCommandBus martenAsyncCommandBus;
    private readonly List<Guid> userIds = [];
    private readonly EventListener eventListener = new();
    private readonly CancellationToken ct = new CancellationTokenSource().Token;
    private readonly KurrentDBClient kurrentDBClient;
    private readonly IHostedService subscriptionToAll;

    public KurrentDBAsyncCommandBusTests(KurrentDBContainerFixture kurrentDB)
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
            .AddKurrentDB(new KurrentDBConfig { ConnectionString = kurrentDB.ConnectionString })
            .AddKurrentDBSubscriptionToAll<EventBusBatchHandler>("AsyncCommandBusTest")
            .AddCommandHandler<AddUser, AddUserCommandHandler>(
                _ => new AddUserCommandHandler(userIds)
            )
            .AddScoped(sp => new InMemoryCommandBus(
                sp,
                new CommandHandlerActivity(new CommandHandlerMetrics(new DummyMeterFactory(), new FakeTimeProvider())),
                new ActivityScope(),
                Policy.NoOpAsync()
            ))
            .AddSingleton(eventListener)
            .AddCommandForwarder();

        var serviceProvider = services.BuildServiceProvider();
        kurrentDBClient = serviceProvider.GetRequiredService<KurrentDBClient>();

        subscriptionToAll = serviceProvider.GetRequiredService<IHostedService>();

        martenAsyncCommandBus = new KurrentDBAsyncCommandBus(kurrentDBClient);
    }

    [Fact]
    public async Task CommandIsStoredInKurrentDBAndForwardedToCommandHandler()
    {
        // Given
        var userId = Guid.CreateVersion7();
        var command = new AddUser(userId);

        // When
        await subscriptionToAll.StartAsync(ct);

        await martenAsyncCommandBus.Schedule(command, ct);

        // Then
        await eventListener.WaitForProcessing(command, ct);

        var commands = await kurrentDBClient.ReadStream(KurrentDBAsyncCommandBus.CommandsStreamId, ct);
        commands.Count().ShouldBeGreaterThanOrEqualTo(1);
        commands.OfType<AddUser>()
            .Count(e => e.UserId == userId)
            .ShouldBe(1);

        userIds.ShouldContain(userId);
    }
}

public record AddUser(Guid UserId, string? Sth = null);

internal class AddUserCommandHandler(List<Guid> userIds): ICommandHandler<AddUser>
{
    public Task Handle(AddUser command, CancellationToken ct)
    {
        userIds.Add(command.UserId);

        return Task.CompletedTask;
    }
}

internal sealed class DummyMeterFactory: IMeterFactory
{
    public Meter Create(MeterOptions options) => new(options);

    public void Dispose() { }
}
