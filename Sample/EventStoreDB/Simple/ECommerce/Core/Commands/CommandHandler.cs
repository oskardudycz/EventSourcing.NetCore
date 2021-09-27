using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Commands
{
    public interface ICommandHandler<in T>
    {
        ValueTask Handle(T command, CancellationToken token);
    }

    public static class CommandHandlerExtensions
    {
        public static async Task HandleCreateCommand<TEntity, TCommand>(
            IEventStoreDBRepository<TEntity> repository,
            Func<TCommand, object> handle,
            Func<TCommand, string> getId,
            TCommand command,
            CancellationToken ct
        ) where TEntity : notnull
        {
            var entityId = getId(command);
            var @event = handle(command);

            await repository.Append(entityId, @event, ct);
        }

        public static async Task HandleUpdateCommand<TEntity, TCommand>(
            IEventStoreDBRepository<TEntity> repository,
            Func<TEntity, TCommand, object> handle,
            Func<TCommand, string> getId,
            Func<TEntity?, object, TEntity> when,
            TCommand command,
            CancellationToken ct
        ) where TEntity : notnull
        {
            var id = getId(command);
            var entity = await repository.Find(when, id, ct);

            var @event = handle(entity, command);

            await repository.Append(id, @event, ct);
        }

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

        public static IServiceCollection AddCreateCommandHandler<TEntity, TCommand>(
            this IServiceCollection services,
            Func<TCommand, object> handle,
            Func<TCommand, string> getId
        ) where TEntity : notnull
            => services
                .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
                {
                    var repository = sp.GetRequiredService<IEventStoreDBRepository<TEntity>>();

                    return async (command, ct) =>
                        await HandleCreateCommand(repository, handle, getId, command, ct);
                });

        public static IServiceCollection AddUpdateCommandHandler<TEntity, TCommand>(
            this IServiceCollection services,
            Func<TEntity, TCommand, object> handle,
            Func<TCommand, string> getId,
            Func<TEntity?, object, TEntity> when
        ) where TEntity : notnull
            => services
                .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
                {
                    var repository = sp.GetRequiredService<IEventStoreDBRepository<TEntity>>();
                    return async (command, ct) =>
                        await HandleUpdateCommand(repository, handle, getId, when, command, ct);
                });


    }
}
