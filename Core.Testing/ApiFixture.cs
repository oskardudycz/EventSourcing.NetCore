using System.Linq.Expressions;
using Core.Api.Testing;
using Core.Commands;
using Core.Events;
using Core.Events.External;
using Core.Requests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Testing;

public abstract class ApiWithEventsFixture<TStartup>: ApiFixture<TStartup> where TStartup : class
{
    private readonly EventsLog eventsLog = new();
    private readonly DummyExternalEventProducer externalEventProducer = new();
    private readonly DummyExternalCommandBus externalCommandBus = new();

    public override TestContext CreateTestContext() =>
        new TestContext<TStartup>(GetConfiguration, services =>
        {
            SetupServices?.Invoke(services);
            services.AddSingleton(eventsLog);
            services.AddSingleton(typeof(IEventHandler<>), typeof(EventListener<>));
            services.AddSingleton<IExternalEventProducer>(externalEventProducer);
            services.AddSingleton<IExternalCommandBus>(externalCommandBus);
            services.AddSingleton<IExternalEventConsumer, DummyExternalEventConsumer>();

        }, SetupWebHostBuilder);



    public IReadOnlyCollection<TEvent> PublishedExternalEventsOfType<TEvent>() where TEvent : IExternalEvent
    {
        return externalEventProducer.PublishedEvents.OfType<TEvent>().ToList();
    }

    public IReadOnlyCollection<TCommand> PublishedExternalCommandOfType<TCommand>() where TCommand : ICommand
    {
        return externalCommandBus.SentCommands.OfType<TCommand>().ToList();
    }

    public async Task PublishInternalEvent(IEvent @event, CancellationToken ct = default)
    {
        using var scope = Server.Host.Services.CreateScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        await eventBus.Publish(@event, ct);
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

public abstract class ApiWithEventsFixture: ApiFixture
{
}
