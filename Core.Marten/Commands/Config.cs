using Core.Commands;
using Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.Commands;

public static class Config
{
    public static IServiceCollection AddMartenAsyncCommandBus<TCommand, TCommandHandler>(
        this IServiceCollection services
    ) where TCommandHandler : class, ICommandHandler<TCommand>
    {
        return services.AddScoped<IAsyncCommandBus, MartenAsyncCommandBus>()
            .AddScoped(typeof(IEventHandler<>), typeof(MartenCommandForwarder<>));
    }
}
