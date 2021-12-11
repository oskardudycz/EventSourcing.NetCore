using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace DataAnalytics.Core.Events
{
    public interface IEventBus
    {
        Task Publish(ResolvedEvent @event, CancellationToken ct);
    }

    public class EventBus: IEventBus
    {
        private readonly IServiceProvider serviceProvider;
        private readonly AsyncRetryPolicy retryPolicy;

        public EventBus(
            IServiceProvider serviceProvider,
            AsyncRetryPolicy retryPolicy
        )
        {
            this.serviceProvider = serviceProvider;
            this.retryPolicy = retryPolicy;
        }

        public async Task Publish(ResolvedEvent @event, CancellationToken ct)
        {
            using var scope = serviceProvider.CreateScope();

            var eventHandlers = scope.ServiceProvider
                .GetServices<Func<IServiceProvider, ResolvedEvent, CancellationToken, Task>>();

            foreach (var handle in eventHandlers)
            {
                await retryPolicy.ExecuteAsync(async token =>
                {
                    await handle(scope.ServiceProvider, @event, token);
                }, ct);
            }
        }
    }

    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services) =>
            services.AddSingleton<IEventBus, EventBus>(sp =>
                new EventBus(
                    sp,
                    Policy.Handle<Exception>().RetryAsync(3)
                )
            );
    }
}
