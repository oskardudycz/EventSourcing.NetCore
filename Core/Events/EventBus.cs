using System.Collections.Concurrent;
using System.Reflection;
using Core.OpenTelemetry;
using Core.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace Core.Events;

public interface IEventBus
{
    Task Publish(IEventEnvelope @event, CancellationToken ct);
}

public class EventBus: IEventBus
{
    private readonly IServiceProvider serviceProvider;
    private readonly Func<IServiceProvider, IEventEnvelope?, TracingScope> createTracingScope;
    private readonly IActivityScope activityScope;
    private readonly AsyncPolicy retryPolicy;
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

    public EventBus(
        IServiceProvider serviceProvider,
        Func<IServiceProvider, IEventEnvelope?, TracingScope> createTracingScope,
        IActivityScope activityScope,
        AsyncPolicy retryPolicy
    )
    {
        this.serviceProvider = serviceProvider;
        this.createTracingScope = createTracingScope;
        this.activityScope = activityScope;
        this.retryPolicy = retryPolicy;
    }

    private async Task Publish<TEvent>(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
        where TEvent : notnull
    {
        using var scope = serviceProvider.CreateScope();
        using var tracingScope = createTracingScope(scope.ServiceProvider, eventEnvelope);

        var eventName = eventEnvelope.Data.GetType().Name;

        var activityOptions = new StartActivityOptions
        {
            ParentId = eventEnvelope.Metadata.Trace?.CausationId?.Value,
            Tags = { { TelemetryTags.EventHandling.Event, eventName } }
        };

        var eventEnvelopeHandlers =
            scope.ServiceProvider.GetServices<IEventHandler<EventEnvelope<TEvent>>>();

        foreach (var eventHandler in eventEnvelopeHandlers)
        {
            var activityName = $"{eventHandler.GetType().Name}/{eventName}";

            await activityScope.Run(
                activityName,
                (_, token) => retryPolicy.ExecuteAsync(c => eventHandler.Handle(eventEnvelope, c), token),
                activityOptions,
                ct
            );
        }

        // publish also just event data
        // thanks to that both handlers with envelope and without will be called
        var eventHandlers =
            scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var eventHandler in eventHandlers)
        {
            var activityName = $"{eventHandler.GetType().Name}/{eventName}";

            await activityScope.Run(
                activityName,
                (_, token) => retryPolicy.ExecuteAsync(c => eventHandler.Handle(eventEnvelope.Data, c), token),
                activityOptions,
                ct
            );
        }
    }

    public Task Publish(IEventEnvelope eventEnvelope, CancellationToken ct)
    {
        return (Task)GetGenericPublishFor(eventEnvelope)
            .Invoke(this, new object[] { eventEnvelope, ct })!;
    }

    private static MethodInfo GetGenericPublishFor(IEventEnvelope @event) =>
        PublishMethods.GetOrAdd(@event.Data.GetType(), eventType =>
            typeof(EventBus)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(Publish) && m.GetGenericArguments().Any())
                .MakeGenericMethod(eventType)
        );
}

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null)
    {
        services.AddSingleton(sp => new EventBus(
            sp,
            sp.GetRequiredService<ITracingScopeFactory>().CreateTraceScope,
            sp.GetRequiredService<IActivityScope>(),
            asyncPolicy ?? Policy.NoOpAsync()
        ));
        services
            .TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());

        return services;
    }
}
