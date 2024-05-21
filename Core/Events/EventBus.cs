using System.Collections.Concurrent;
using System.Reflection;
using Core.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace Core.Events;

public interface IEventBus
{
    Task Publish(IEventEnvelope @event, CancellationToken ct);
}

public class EventBus(
    IServiceProvider serviceProvider,
    IActivityScope activityScope,
    AsyncPolicy retryPolicy
): IEventBus
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

    private async Task Publish<TEvent>(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
        where TEvent : notnull
    {
        using var scope = serviceProvider.CreateScope();

        var eventName = eventEnvelope.Data.GetType().Name;

        var activityOptions = new StartActivityOptions { Tags = { { TelemetryTags.EventHandling.Event, eventName } } };

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
            ).ConfigureAwait(false);
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
            ).ConfigureAwait(false);
        }
    }

    public Task Publish(IEventEnvelope eventEnvelope, CancellationToken ct)
    {
        return (Task)GetGenericPublishFor(eventEnvelope)
            .Invoke(this, [eventEnvelope, ct])!;
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
            sp.GetRequiredService<IActivityScope>(),
            asyncPolicy ?? Policy.NoOpAsync()
        ));
        services.AddScoped<EventBusBatchHandler, EventBusBatchHandler>();
        services.AddScoped<IEventBatchHandler>(sp => sp.GetRequiredService<EventBusBatchHandler>());
        services
            .TryAddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());

        return services;
    }
}
