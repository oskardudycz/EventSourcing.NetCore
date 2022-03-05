using Microsoft.Extensions.DependencyInjection;

namespace Core.Events;

public static class Config
{
    public static IServiceCollection AddEventHandler<TEvent, TEventHandler>(
        this IServiceCollection services
    )
        where TEvent : IEvent
        where TEventHandler : class, IEventHandler<TEvent>
    {
        return services
            .AddTransient<TEventHandler>()
            .AddTransient<IEventHandler<TEvent>>(sp => sp.GetRequiredService<TEventHandler>());
    }
}
