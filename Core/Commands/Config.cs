using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Commands;

public static class Config
{
    public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services
    )
        where TCommand : ICommand
        where TCommandHandler : class, ICommandHandler<TCommand>
    {
        return services.AddTransient<TCommandHandler>()
            .AddTransient<IRequestHandler<TCommand, Unit>>(sp => sp.GetRequiredService<TCommandHandler>())
            .AddTransient<ICommandHandler<TCommand>>(sp => sp.GetRequiredService<TCommandHandler>());
    }
}