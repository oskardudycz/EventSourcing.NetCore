using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Core;
using Core.Commands;
using Core.Events;
using Core.Marten;
using Core.Marten.Commands;
using Core.OpenTelemetry;
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

    public MartenAsyncCommandBusTests(): base(false)
    {
        var services = new ServiceCollection();

        services
            .AddLogging()
            .AddSingleton<IHostEnvironment, HostingEnvironment>(
                _ => new HostingEnvironment { EnvironmentName = Environments.Development }
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
            .AddSingleton<EventListener>()
            .AddScoped(typeof(IEventHandler<>), typeof(EventCatcher<>));

        var serviceProvider = services.BuildServiceProvider();
        Session = serviceProvider.GetRequiredService<IDocumentSession>();

        asyncDaemon = (AsyncProjectionHostedService)serviceProvider.GetRequiredService<IHostedService>();

        martenAsyncCommandBus = new MartenAsyncCommandBus(Session);
    }

    [Fact(Skip = "sounds like deadlock")]
    public async Task CommandIsStoredInMarten()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        // Given
        var userId = Guid.NewGuid();
        var command = new AddUser(userId);

        // When
        await asyncDaemon.StartAsync(default);
        await martenAsyncCommandBus.Schedule(command, ct);

        // Then
        await eventListener.WaitForProcessing(command, ct);

        var events = await Session.Events.FetchStreamAsync(MartenAsyncCommandBus.CommandsStreamId, token: ct);
        events.Should().HaveCountGreaterThanOrEqualTo(1);
        events.Select(e => e.Data).OfType<AddUser>()
            .Count(e => e.UserId == userId)
            .Should().Be(1);
    }


    [Fact]
    public async Task EventListener()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        // Given
        var userId = Guid.NewGuid();
        var command = new AddUser(userId);

        object? result = null;

        await Task.WhenAll(
            Task.Run(async () =>
            {
                result = await eventListener.WaitForProcessing(command, ct);
            }, ct),
            eventListener.Handle(command, ct)
        );

        result.Should().NotBeNull();
        result.Should().Be(command);
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

public class EventListener
{
    private readonly Channel<object> channel = Channel.CreateUnbounded<object>();

    public async Task Handle(object @event, CancellationToken ct)
    {
        await channel.Writer.WriteAsync(@event, ct);
    }

    public async Task<object> WaitForProcessing(object @event, CancellationToken ct)
    {
        await foreach (var item in channel.Reader.ReadAllAsync(ct))
        {
            if (item.Equals(@event))
                return item;
        }

        throw new Exception("No events were found");
    }
}

public class EventCatcher<T>: IEventHandler<T>
{
    private readonly EventListener listener;

    public EventCatcher(EventListener listener) =>
        this.listener = listener;

    public Task Handle(T @event, CancellationToken ct)
    {
        return listener.Handle(@event!, ct);
    }
}
