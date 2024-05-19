namespace Core.Commands;

public static class Config
{
    public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services,
        Func<IServiceProvider,TCommandHandler> create
    ) where TCommandHandler : class, ICommandHandler<TCommand> =>
        services.AddTransient<TCommandHandler>()
            .AddTransient<ICommandHandler<TCommand>>(create);

    public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services
    ) where TCommandHandler : class, ICommandHandler<TCommand> =>
        services.AddCommandHandler<TCommand, TCommandHandler>(sp => sp.GetRequiredService<TCommandHandler>());
}
