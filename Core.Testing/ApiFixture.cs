using System.Linq.Expressions;
using Core.Api.Testing;
using Core.Events;
using Core.Events.External;
using Core.Requests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Testing;

public abstract class ApiWithEventsFixture<TProject>: ApiFixture<TProject> where TProject : class
{
    private readonly EventsLog eventsLog = new();
    private readonly DummyExternalEventProducer externalEventProducer = new();
    private readonly DummyExternalCommandBus externalCommandBus = new();

    public override TestContext<TProject> CreateTestContext() =>
        new(services =>
        {
            SetupServices?.Invoke(services);
            services.AddSingleton(eventsLog);
            services.AddSingleton(typeof(IEventHandler<>), typeof(EventListener<>));
            services.AddSingleton<IExternalEventProducer>(externalEventProducer);
            services.AddSingleton<IEventBus>(sp =>
                new EventBusDecoratorWithExternalProducer(sp.GetRequiredService<EventBus>(),
                    sp.GetRequiredService<IExternalEventProducer>()));
            services.AddSingleton<IExternalCommandBus>(externalCommandBus);
            services.AddSingleton<IExternalEventConsumer, DummyExternalEventConsumer>();
        });


    public void PublishedExternalEventsOfType<TEvent>() where TEvent : IExternalEvent
    {
        externalEventProducer.PublishedEvents.OfType<TEvent>().ToList().Should().NotBeEmpty();
    }

    public Task PublishInternalEvent<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : notnull =>
        PublishInternalEvent(
            new EventEnvelope<TEvent>(@event, new EventMetadata(Guid.NewGuid().ToString(), 0, 0, null)), ct);

    public async Task PublishInternalEvent<TEvent>(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct = default)
        where TEvent : notnull
    {
        using var scope = Sut.Services.CreateScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        await eventBus.Publish(eventEnvelope, ct);
    }

    public IReadOnlyCollection<TEvent> PublishedInternalEventsOfType<TEvent>() =>
        eventsLog.PublishedEvents.OfType<TEvent>().ToList();

    // TODO: Add Poly here
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

            await Task.Delay(retryIntervalInMs);
            retryCount--;
        } while (!finished);
    }
}
