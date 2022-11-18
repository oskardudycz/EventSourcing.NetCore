using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Warehouse.Api.Core.Commands;

public interface ICommandHandler<in T>
{
    Task Handle(T command, CancellationToken token);
}

public delegate Task CommandHandler<in T>(T query, CancellationToken ct);

public static class CommandHandlerConfiguration
{
    public static IServiceCollection AddCommandHandler<T, TCommandHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TCommandHandler>? configure = null
    ) where TCommandHandler : class, ICommandHandler<T>
    {
        if (configure == null)
        {
            services.AddTransient<TCommandHandler, TCommandHandler>();
            services.AddTransient<ICommandHandler<T>, TCommandHandler>();
        }
        else
        {
            services.AddTransient<TCommandHandler, TCommandHandler>(configure);
            services.AddTransient<ICommandHandler<T>, TCommandHandler>(configure);
        }

        services
            .AddTransient<Func<T, CancellationToken, Task>>(
                sp => sp.GetRequiredService<ICommandHandler<T>>().Handle
            )
            .AddTransient<CommandHandler<T>>(
                sp => sp.GetRequiredService<ICommandHandler<T>>().Handle
            );

        return services;
    }
}
