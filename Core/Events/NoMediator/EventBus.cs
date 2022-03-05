using System.Collections.Concurrent;
using System.Reflection;
using Core.Events.External;
using Core.Tracing;
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
    private readonly Func<IServiceProvider, EventEnvelope?, TracingScope> createTracingScope;
    private readonly AsyncPolicy retryPolicy;
    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethods = new();

    public NoMediatorEventBus(
        IServiceProvider serviceProvider,
        Func<IServiceProvider, EventEnvelope?, TracingScope> createTracingScope,
        AsyncPolicy retryPolicy
    )
    {
        this.serviceProvider = serviceProvider;
        this.createTracingScope = createTracingScope;
        this.retryPolicy = retryPolicy;
    }

    private async Task Publish<TEvent>(TEvent @event, CancellationToken ct)
    {
        var eventEnvelope = @event as EventEnvelope;
        // You can consider adding here a retry policy for event handling
        using var scope = serviceProvider.CreateScope();
        using var tracingScope = createTracingScope(serviceProvider, eventEnvelope);

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

    public async Task Publish(object @event, CancellationToken ct)
    {
        // if it's an event envelope, publish also just event data
        // thanks to that both handlers with envelope and without will be called
        if (@event is EventEnvelope(var data, _))
            await (Task)GetGenericPublishFor(data)
                .Invoke(this, new[] { data, ct })!;

        await (Task)GetGenericPublishFor(@event)
            .Invoke(this, new[] { @event, ct })!;
    }

    private static MethodInfo GetGenericPublishFor(object @event) =>
        PublishMethods.GetOrAdd(@event.GetType(), eventType =>
            typeof(NoMediatorEventBus)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(Publish) && m.GetGenericArguments().Any())
                .MakeGenericMethod(eventType)
        );
}

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null) =>
        services.AddSingleton<INoMediatorEventBus, NoMediatorEventBus>(sp =>
            new NoMediatorEventBus(
                sp,
                sp.GetRequiredService<ITracingScopeFactory>().CreateTraceScope,
                asyncPolicy ?? Policy.NoOpAsync()
            )
        );
}
