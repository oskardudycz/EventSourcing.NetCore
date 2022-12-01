using System.Linq.Expressions;
using Core.Events;
using Core.Events.External;
using Core.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Core.Testing;

public class TestWebApplicationFactory<TProject>: WebApplicationFactory<TProject> where TProject : class
{
    private readonly EventsLog eventsLog = new();
    private readonly DummyExternalEventProducer externalEventProducer = new();
    private readonly DummyExternalCommandBus externalCommandBus = new();

    private readonly string schemaName = $"test{Guid.NewGuid().ToString("N").ToLower()}";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(eventsLog)
                .AddSingleton<IExternalEventProducer>(externalEventProducer)
                .AddSingleton<IEventBus>(sp =>
                    new EventListener(eventsLog,
                        new EventBusDecoratorWithExternalProducer(sp.GetRequiredService<EventBus>(),
                            sp.GetRequiredService<IExternalEventProducer>())
                    )
                )
                .AddSingleton<IExternalCommandBus>(externalCommandBus)
                .AddSingleton<IExternalEventConsumer, DummyExternalEventConsumer>();
        });


        Environment.SetEnvironmentVariable("SchemaName", schemaName);

        return base.CreateHost(builder);
    }

    public void PublishedExternalEventsOfType<TEvent>() where TEvent : IExternalEvent =>
        externalEventProducer.PublishedEvents.OfType<TEvent>().ToList().Should().NotBeEmpty();

    public Task PublishInternalEvent<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : notnull =>
        PublishInternalEvent(
            new EventEnvelope<TEvent>(@event, new EventMetadata(Guid.NewGuid().ToString(), 0, 0, null)), ct);

    public async Task PublishInternalEvent<TEvent>(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct = default)
        where TEvent : notnull
    {
        using var scope = Services.CreateScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        await eventBus.Publish(eventEnvelope, ct).ConfigureAwait(false);
    }

    public IReadOnlyCollection<TEvent> PublishedInternalEventsOfType<TEvent>() =>
        eventsLog.PublishedEvents.OfType<TEvent>().ToList();

    public async Task ShouldPublishInternalEventOfType<TEvent>(
        Expression<Func<TEvent, bool>> predicate,
        int maxNumberOfRetries = 5,
        int retryIntervalInMs = 1000)
    {
        var retryCount = maxNumberOfRetries;
        var finished = false;

        do
        {
            try
            {
                PublishedInternalEventsOfType<TEvent>().Should()
                    .HaveCount(1)
                    .And.Contain(predicate);

                finished = true;
            }
            catch
            {
                if (retryCount == 0)
                    throw;
            }

            await Task.Delay(retryIntervalInMs).ConfigureAwait(false);
            retryCount--;
        } while (!finished);
    }
}
