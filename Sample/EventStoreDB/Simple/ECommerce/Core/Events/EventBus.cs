using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
        private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

        public EventBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task Publish<TEvent>(TEvent @event, CancellationToken ct)
        {
            var eventHandlers = serviceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var eventHandler in eventHandlers)
            {
                await eventHandler.Handle(@event, ct);
            }
        }

        public Task Publish(object @event, CancellationToken ct)
        {
            return (Task)GetGenericPublishFor(@event)
                .Invoke(this, new[] {@event, ct})!;
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
}
