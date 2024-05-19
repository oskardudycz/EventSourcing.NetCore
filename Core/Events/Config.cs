using Microsoft.Extensions.DependencyInjection;

namespace Core.Events;

public static class Config
{
    public static IServiceCollection AddEventHandler<TEvent, TEventHandler>(
        this IServiceCollection services
    )
        where TEventHandler : class, IEventHandler<TEvent> =>
        services
            .AddTransient<TEventHandler>()
            .AddTransient<IEventHandler<TEvent>>(sp => sp.GetRequiredService<TEventHandler>());

    public static IServiceCollection AddEventHandler<TEvent>(
        this IServiceCollection services,
        Func<IServiceProvider, TEvent, CancellationToken, Task> handler
    ) =>
        services
            .AddTransient<IEventHandler<TEvent>>(sp => new EventHandler<TEvent>((e, ct) => handler(sp, e, ct)));

    public static IServiceCollection AddEventHandler<TEvent>(
        this IServiceCollection services,
        Func<TEvent, CancellationToken, Task> handler
    ) =>
        services
            .AddTransient<IEventHandler<TEvent>>(_ => new EventHandler<TEvent>(handler));
}
