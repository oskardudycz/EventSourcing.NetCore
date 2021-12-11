using System;
using System.Threading;
using System.Threading.Tasks;
using DataAnalytics.Core.Serialisation;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace DataAnalytics.Core.Events
{
    public static class EventHandler
    {
        public static IServiceCollection AddEventHandler(
            this IServiceCollection services,
            Func<ResolvedEvent, CancellationToken, Task> handler,
            string? eventType = null
        ) =>
            services.AddEventHandler(
                (_, resolvedEvent, ct) => handler(resolvedEvent, ct), eventType);

        public static IServiceCollection AddEventHandler(
            this IServiceCollection services,
            Func<IServiceProvider, ResolvedEvent, CancellationToken, Task> handler,
            string? eventType = null
        )
        {
            services.AddScoped<Func<IServiceProvider, ResolvedEvent, CancellationToken, Task>>(
                _ => (sp, resolvedEvent, ct) =>
                {
                    if (eventType != null && resolvedEvent.Event.EventType != eventType)
                        return Task.CompletedTask;

                    return handler(sp, resolvedEvent, ct);
                }
            );

            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent>(
            this IServiceCollection services,
            Func<TEvent, CancellationToken, Task> handler
        ) =>
            services.AddEventHandler<TEvent>(
                (_, @event, ct) => handler(@event, ct));

        public static IServiceCollection AddEventHandler<TEvent>(
            this IServiceCollection services,
            Func<IServiceProvider, TEvent, CancellationToken, Task> handler
        )
        {
            var eventType = EventTypeMapper.ToName<TEvent>();

            services.AddScoped<Func<IServiceProvider, ResolvedEvent, CancellationToken, Task>>(
                _ => (sp, resolvedEvent, ct) =>
                {
                    if (resolvedEvent.Event.EventType != eventType)
                        return Task.CompletedTask;

                    var @event = resolvedEvent.DeserializeData<TEvent>();

                    return handler(sp, @event, ct);
                }
            );

            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent, TEventMetadata>(
            this IServiceCollection services,
            Func<TEvent, TEventMetadata, CancellationToken, Task> handler
        ) =>
            services.AddEventHandler<TEvent, TEventMetadata>(
                (_, @event, metadata, ct) => handler(@event, metadata, ct));

        public static IServiceCollection AddEventHandler<TEvent, TEventMetadata>(
            this IServiceCollection services,
            Func<IServiceProvider, TEvent, TEventMetadata, CancellationToken, Task> handler
        )
        {
            var eventType = EventTypeMapper.ToName<TEvent>();

            services.AddScoped<Func<IServiceProvider, ResolvedEvent, CancellationToken, Task>>(
                _ => (sp, resolvedEvent, ct) =>
                {
                    if (resolvedEvent.Event.EventType != eventType)
                        return Task.CompletedTask;

                    var @event = resolvedEvent.DeserializeData<TEvent>();
                    var metadata = resolvedEvent.DeserializeMetadata<TEventMetadata>();

                    return handler(sp, @event, metadata, ct);
                }
            );

            return services;
        }
    }
}
