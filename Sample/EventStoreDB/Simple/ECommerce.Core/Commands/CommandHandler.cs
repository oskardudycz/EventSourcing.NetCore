using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.EventStoreDB;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Commands;

public static class CommandHandlerExtensions
{
    public static async Task HandleCreateCommand<TCommand, TEntity>(
        EventStoreClient eventStore,
        Func<TCommand, object> handle,
        Func<TCommand, string> getId,
        TCommand command,
        CancellationToken ct
    ) where TEntity : notnull
    {
        var entityId = getId(command);
        var @event = handle(command);

        await eventStore.Append(entityId, @event, ct);
    }

    public static async Task HandleUpdateCommand<TCommand, TEntity>(
        EventStoreClient eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<TCommand, TEntity, object> handle,
        Func<TCommand, string> getId,
        Func<TCommand, uint> getVersion,
        TCommand command,
        CancellationToken ct) where TEntity : notnull
    {
        var id = getId(command);
        var entity = await eventStore.Find(getDefault, when, id, ct);

        var @event = handle(command, entity);

        await eventStore.Append(id, @event, getVersion(command), ct);
    }

    public static IServiceCollection AddCreateCommandHandler<TCommand, TEntity>(
        this IServiceCollection services,
        Func<TCommand, object> handle,
        Func<TCommand, string> getId
    ) where TEntity : notnull =>
        services.AddCreateCommandHandler<TCommand, TEntity>(_ => handle, getId);

    public static IServiceCollection AddCreateCommandHandler<TCommand, TEntity>(
        this IServiceCollection services,
        Func<IServiceProvider, Func<TCommand, object>> handle,
        Func<TCommand, string> getId
    ) where TEntity : notnull =>
        services
            .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
            {
                var eventStore = sp.GetRequiredService<EventStoreClient>();

                return async (command, ct) =>
                    await HandleCreateCommand<TCommand, TEntity>(eventStore, handle(sp), getId, command, ct);
            });

    public static IServiceCollection AddUpdateCommandHandler<TCommand, TEntity>(
        this IServiceCollection services,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<TCommand, TEntity, object> handle,
        Func<TCommand, string> getId,
        Func<TCommand, uint> getVersion) where TEntity : notnull =>
        services.AddUpdateCommandHandler(getDefault, when, _ => handle, getId, getVersion);

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
                var repository = sp.GetRequiredService<EventStoreClient>();
                return async (command, ct) =>
                    await HandleUpdateCommand(repository, getDefault, when, handle(sp), getId, getVersion, command,
                        ct);
            });

    public static IServiceCollection For<TEntity>(
        this IServiceCollection services,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Action<CommandHandlersBuilder<TEntity>> setup
    )
        where TEntity : class
    {
        setup(new CommandHandlersBuilder<TEntity>(services, getDefault, when));
        return services;
    }

    public class CommandHandlersBuilder<TEntity>
        where TEntity : class
    {
        public readonly IServiceCollection services;
        private readonly Func<TEntity> getDefault;
        private readonly Func<TEntity, object, TEntity> when;

        public CommandHandlersBuilder(
            IServiceCollection services,
            Func<TEntity> getDefault,
            Func<TEntity, object, TEntity> when
        )
        {
            this.services = services;
            this.getDefault = getDefault;
            this.when = when;
        }

        public CommandHandlersBuilder<TEntity> AddOn<TCommand>(
            Func<TCommand, object> handle,
            Func<TCommand, string> getId
        )
        {
            services.AddCreateCommandHandler<TCommand, TEntity>(_ => handle, getId);
            return this;
        }

        public CommandHandlersBuilder<TEntity> AddOn<TCommand>(
            Func<IServiceProvider, Func<TCommand, object>> handle,
            Func<TCommand, string> getId
        )
        {
            services.AddCreateCommandHandler<TCommand, TEntity>(_ => handle, getId);
            return this;
        }

        public CommandHandlersBuilder<TEntity> UpdateOn<TCommand>(
            Func<TCommand, TEntity, object> handle,
            Func<TCommand, string> getId,
            Func<TCommand, uint> getVersion)
        {
            services.AddUpdateCommandHandler(getDefault, when, _ => handle, getId, getVersion);
            return this;
        }

        public CommandHandlersBuilder<TEntity> UpdateOn<TCommand>(
            Func<IServiceProvider, Func<TCommand, TEntity, object>> handle,
            Func<TCommand, string> getId,
            Func<TCommand, uint> getVersion
        )
        {
            services.AddUpdateCommandHandler(getDefault, when, handle, getId, getVersion);
            return this;
        }
    }
}