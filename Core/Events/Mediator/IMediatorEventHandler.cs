using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Events.Mediator;

public interface IMediatorEventHandler<in TEvent>: INotificationHandler<TEvent>
    where TEvent : IEvent
{
}

public static class MediatorEventHandlerExtensions
{
    public static IServiceCollection AddMediatorEventHandler<TEvent, TEventHandler>(
        this IServiceCollection services
    )
        where TEvent : IEvent
        where TEventHandler : class, IMediatorEventHandler<TEvent>
    {
        return services.AddTransient<TEventHandler>()
            .AddTransient<INotificationHandler<TEvent>>(sp => sp.GetRequiredService<TEventHandler>())
            .AddTransient<IMediatorEventHandler<TEvent>>(sp => sp.GetRequiredService<TEventHandler>());
    }
}
