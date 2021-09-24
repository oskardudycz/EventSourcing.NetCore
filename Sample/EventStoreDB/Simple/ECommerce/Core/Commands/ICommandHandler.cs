using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Commands
{
    public interface ICommandHandler<in T>
    {
        ValueTask Handle(T command, CancellationToken token);
    }

    public static class CommandHandlerExtensions
    {
        public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(this IServiceCollection services)
            where TCommandHandler: class, ICommandHandler<TCommand>
            => services
                .AddTransient<ICommandHandler<TCommand>, TCommandHandler>()
                .AddTransient<Func<TCommand, CancellationToken, ValueTask>>((sp) =>
                    async (command, ct) =>
                    {
                        var commandHandler = sp.GetRequiredService<ICommandHandler<TCommand>>();
                        await commandHandler.Handle(command, ct);
                    });
    }
}
