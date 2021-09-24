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
        public static IServiceCollection AddCreateCommandHandler<TEntity, TCommand>(
            this IServiceCollection services,
            Func<TCommand, object> handle,
            Func<TCommand, Guid> getId
        ) where TEntity : notnull
            => services
                .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
                {
                    var repository = sp.GetRequiredService<IEventStoreDBRepository<TEntity>>();
                    return async (command, ct) =>
                    {


                        var entityId = getId(command);
                        var @event = handle(command);

                        await repository.Append(entityId, @event, ct);
                    };
                });


        public static IServiceCollection AddUpdateCommandHandler<TEntity, TCommand>(
            this IServiceCollection services,
            Func<TEntity, TCommand, object> handle,
            Func<TCommand, Guid> getId,
            Func<TEntity, object, TEntity> when
        ) where TEntity : notnull
            => services
                .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
                {
                    var repository = sp.GetRequiredService<IEventStoreDBRepository<TEntity>>();
                    return async (command, ct) =>
                    {
                        var entityId = getId(command);
                        var entity = await repository.Find(entityId, when, ct);

                        var @event = handle(entity, command);

                        await repository.Append(getId(command), @event, ct);
                    };
                });
    }
}
