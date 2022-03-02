using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Api.Testing;
using Core.Commands;
using Core.Events;
using Core.Events.External;
using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IEventBus = Core.Events.IEventBus;

namespace Core.Testing;

public abstract class ApiWithEventsFixture<TStartup>: ApiFixture<TStartup> where TStartup : class
{
    private readonly EventsLog eventsLog = new();
    private readonly DummyExternalEventProducer externalEventProducer = new();
    private readonly DummyExternalCommandBus externalCommandBus = new();

    public override TestContext CreateTestContext() =>
        new TestContext<TStartup>(GetConfiguration, (services) =>
        {
            SetupServices?.Invoke(services);
            services.AddSingleton(eventsLog);
            services.AddSingleton(typeof(INotificationHandler<>), typeof(EventListener<>));
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

    public IReadOnlyCollection<TEvent> PublishedInternalEventsOfType<TEvent>()
    {
        return eventsLog.PublishedEvents.OfType<TEvent>().ToList();
    }
}

public abstract class ApiWithEventsFixture: ApiFixture
{
}
