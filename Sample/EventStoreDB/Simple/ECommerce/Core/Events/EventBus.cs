using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace ECommerce.Core.Events
{
    public interface IEventBus
    {
        Task Publish<TEvent>(TEvent @event, CancellationToken ct);

        Task Publish(object @event, CancellationToken ct);
    }

    public class EventBus: IEventBus
    {
        private readonly IServiceProvider serviceProvider;
        private readonly AsyncRetryPolicy retryPolicy;
        private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

        public EventBus(
            IServiceProvider serviceProvider,
            AsyncRetryPolicy retryPolicy
        )
        {
            this.serviceProvider = serviceProvider;
            this.retryPolicy = retryPolicy;
        }

        public async Task Publish<TEvent>(TEvent @event, CancellationToken ct)
        {
            var eventHandlerTypes = GetAllEventHandlerTypesFor(typeof(TEvent));

            foreach (var eventHandlerType in eventHandlerTypes)
            {
                await retryPolicy.ExecuteAsync(async token =>
                {
                    using var scope = serviceProvider.CreateScope();

                    var eventHandler = (IEventHandler<TEvent>)
                        scope.ServiceProvider.GetRequiredService(eventHandlerType);

                    await eventHandler.Handle(@event, token);
                }, ct);
            }
        }

        public Task Publish(object @event, CancellationToken ct)
        {
            return (Task)GetGenericPublishFor(@event)
                .Invoke(this, new[] { @event, ct })!;
        }

        private static MethodInfo GetGenericPublishFor(object @event)
        {
            return PublishMethods.GetOrAdd(@event.GetType(), eventType =>
                typeof(EventBus)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Single(m => m.Name == nameof(Publish) && m.GetGenericArguments().Any())
                    .MakeGenericMethod(eventType)
            );
        }

        private static IEnumerable<Type> GetAllEventHandlerTypesFor(Type eventType)
        {
            var generic = typeof(IEventHandler<>);
            var eventHandlerInterface = generic.MakeGenericType(eventType);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                    a.GetTypes()
                        .Where(type => eventHandlerInterface.IsAssignableFrom(type) && !type.IsInterface)
                );
        }
    }

    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services)
            => services.AddSingleton<IEventBus, EventBus>(sp =>
                new EventBus(sp, Policy.Handle<Exception>().RetryAsync(3)));
    }
}
