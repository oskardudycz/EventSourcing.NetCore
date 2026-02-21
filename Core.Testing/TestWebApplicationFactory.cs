using Core.Events;
using Core.Events.External;
using Core.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Open.ChannelExtensions;
using Polly;

namespace Core.Testing;

public class TestWebApplicationFactory<TProject>: WebApplicationFactory<TProject> where TProject : class
{
    private readonly DummyExternalEventProducer externalEventProducer = new();
    private readonly DummyExternalCommandBus externalCommandBus = new();
    private readonly EventListener eventListener = new();

    private readonly string schemaName = $"test{Guid.CreateVersion7().ToString("N").ToLower()}";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services
                .AddSingleton<IExternalEventProducer>(externalEventProducer)
                .AddSingleton<IEventBus>(sp =>
                    new EventCatcher(
                        eventListener,
                        new EventBusDecoratorWithExternalProducer(sp.GetRequiredService<EventBus>(),
                            sp.GetRequiredService<IExternalEventProducer>())
                    )
                )
                .AddSingleton(eventListener)
                .AddSingleton<IExternalCommandBus>(externalCommandBus)
                .AddSingleton<IExternalEventConsumer, DummyExternalEventConsumer>();
        });

        Environment.SetEnvironmentVariable("ConnectionStrings__kafka", "localhost:9092");
        Environment.SetEnvironmentVariable("SchemaName", schemaName);

        return Policy.Handle<Exception>()
            .WaitAndRetry(5, _ => TimeSpan.FromMilliseconds(500))
            .Execute(() => base.CreateHost(builder));
    }

    public void PublishedExternalEventsOfType<TEvent>() where TEvent : IExternalEvent =>
        externalEventProducer.PublishedEvents.OfType<TEvent>().Should().NotBeEmpty();

    public Task PublishInternalEvent<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : notnull =>
        PublishInternalEvent(
            new EventEnvelope<TEvent>(@event, new EventMetadata(Guid.CreateVersion7().ToString(), 0, 0, null)), ct);

    public async Task PublishInternalEvent<TEvent>(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct = default)
        where TEvent : notnull
    {
        using var scope = Services.CreateScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        await eventBus.Publish(eventEnvelope, ct).ConfigureAwait(false);
    }


    public IAsyncEnumerable<TEvent> PublishedInternalEventsOfType<TEvent>(
        CancellationToken ct = default
    ) =>
        PublishedInternalEventsOfType((TEvent _) => true, ct);

    public IAsyncEnumerable<TEvent> PublishedInternalEventsOfType<TEvent>(
        Func<TEvent, bool> predicate,
        CancellationToken ct = default
    ) =>
        eventListener.Reader
            .Filter(x => x is TEvent)
            .Transform(x => (TEvent)x)
            .Filter(predicate)
            .ReadAllAsync(ct);


    public async Task ShouldPublishInternalEventOfType<TEvent>(
        Func<TEvent, bool> predicate,
        long timeout = 5000
    )
    {
        try
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));

            await foreach (var _ in PublishedInternalEventsOfType(predicate, cts.Token).ConfigureAwait(false))
            {
                return;
            }
        }
        catch (OperationCanceledException)
        {
            0.Should().Be(1, "No events matching criteria were published");
        }
    }
}
