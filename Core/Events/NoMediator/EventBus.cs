using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Core.Events.NoMediator;

public interface INoMediatorEventBus
{
    Task Publish(object @event, CancellationToken ct);
}

public class NoMediatorEventBus: INoMediatorEventBus
{
    private readonly IServiceProvider serviceProvider;
    private readonly AsyncPolicy retryPolicy;
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

    public NoMediatorEventBus(
        IServiceProvider serviceProvider,
        AsyncPolicy retryPolicy
    )
    {
        this.serviceProvider = serviceProvider;
        this.retryPolicy = retryPolicy;
    }

    private async Task Publish<TEvent>(TEvent @event, CancellationToken ct)
    {
        // You can consider adding here a retry policy for event handling
        using var scope = serviceProvider.CreateScope();

        var eventHandlers =
            scope.ServiceProvider.GetServices<INoMediatorEventHandler<TEvent>>();

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
            typeof(NoMediatorEventBus)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(Publish) && m.GetGenericArguments().Any())
                .MakeGenericMethod(eventType)
        );
    }
}

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null) =>
        services.AddSingleton<INoMediatorEventBus, NoMediatorEventBus>(sp =>
            new NoMediatorEventBus(sp, asyncPolicy ?? Policy.NoOpAsync())
        );
}
