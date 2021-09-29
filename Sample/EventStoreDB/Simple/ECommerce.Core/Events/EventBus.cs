using System;
using System.Collections.Concurrent;
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
            using var scope = serviceProvider.CreateScope();

            var eventHandlers =
                scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var eventHandler in eventHandlers)
            {
                await retryPolicy.ExecuteAsync(async token =>
                {
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
    }

    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services)
            => services.AddSingleton<IEventBus, EventBus>(sp =>
                new EventBus(sp, Policy.Handle<Exception>().RetryAsync(3)));
    }
}
