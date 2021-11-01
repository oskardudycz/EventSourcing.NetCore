using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Commands
{
    public static class CommandHandlerExtensions
    {
        public static async Task HandleCreateCommand<TCommand, TEntity>(
            EventStoreDBRepository<TEntity> repository,
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

        public static IServiceCollection AddCreateCommandHandler<TCommand, TEntity>(
            this IServiceCollection services,
            Func<TCommand, object> handle,
            Func<TCommand, string> getId
        ) where TEntity : notnull =>
            AddCreateCommandHandler<TCommand, TEntity>(services, _ => handle, getId);

        public static IServiceCollection AddCreateCommandHandler<TCommand, TEntity>(
            this IServiceCollection services,
            Func<IServiceProvider, Func<TCommand, object>> handle,
            Func<TCommand, string> getId
        ) where TEntity : notnull =>
            services
                .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
                {
                    var repository = sp.GetRequiredService<EventStoreDBRepository<TEntity>>();

                    return async (command, ct) =>
                        await HandleCreateCommand(repository, handle(sp), getId, command, ct);
                });

        public static IServiceCollection AddUpdateCommandHandler<TCommand, TEntity>(
            this IServiceCollection services,
            Func<TEntity> getDefault,
            Func<TEntity, object, TEntity> when,
            Func<TCommand, TEntity, object> handle,
            Func<TCommand, string> getId,
            Func<TCommand, uint> getVersion) where TEntity : notnull =>
            AddUpdateCommandHandler(services, getDefault, when, _ => handle, getId, getVersion);

        public static IServiceCollection AddUpdateCommandHandler<TCommand, TEntity>(
            this IServiceCollection services,
            Func<TEntity> getDefault,
            Func<TEntity, object, TEntity> when,
            Func<IServiceProvider, Func<TCommand, TEntity, object>> handle,
            Func<TCommand, string> getId,
            Func<TCommand, uint> getVersion) where TEntity : notnull =>
            services
                .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
                {
                    var repository = sp.GetRequiredService<EventStoreDBRepository<TEntity>>();
                    return async (command, ct) =>
                        await HandleUpdateCommand(repository, getDefault, when, handle(sp), getId, getVersion, command, ct);
                });

        public static async Task HandleUpdateCommand<TCommand, TEntity>(
            EventStoreDBRepository<TEntity> repository,
            Func<TEntity> getDefault,
            Func<TEntity, object, TEntity> when,
            Func<TCommand, TEntity, object> handle,
            Func<TCommand, string> getId,
            Func<TCommand, uint> getVersion,
            TCommand command,
            CancellationToken ct) where TEntity : notnull
        {
            var id = getId(command);
            var entity = await repository.Find(getDefault, when, id, ct);

            var @event = handle(command, entity);

            await repository.Append(id, @event, getVersion(command), ct);
        }
    }
}
