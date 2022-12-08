using Core.OptimisticConcurrency;
using ECommerce.Core.EventStoreDB;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Commands;

public static class CommandHandlerExtensions
{
    public static Task<ulong> HandleCreateCommand<TCommand>(EventStoreClient eventStore,
        Func<TCommand, object> handle,
        Func<TCommand, string> getId,
        TCommand command,
        CancellationToken ct)
    {
        var entityId = getId(command);
        var @event = handle(command);

        return eventStore.Append(entityId, @event, ct);
    }

    public static async Task<ulong> HandleUpdateCommand<TCommand, TEntity>(
        EventStoreClient eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<TCommand, TEntity, object> handle,
        Func<TCommand, string> getId,
        TCommand command,
        ulong expectedVersion,
        CancellationToken ct) where TEntity : notnull
    {
        var id = getId(command);
        var entity = await eventStore.Find(getDefault, when, id, ct);

        var @event = handle(command, entity);

        return await eventStore.Append(id, @event, expectedVersion, ct);
    }

    public static IServiceCollection AddCreateCommandHandler<TCommand>(
        this IServiceCollection services,
        Func<TCommand, object> handle,
        Func<TCommand, string> getId,
        Action<ulong>? setNextExpectedVersion = null
    ) =>
        services.AddCreateCommandHandler(_ => handle, getId, setNextExpectedVersion);

    public static IServiceCollection AddCreateCommandHandler<TCommand>(
        this IServiceCollection services,
        Func<IServiceProvider, Func<TCommand, object>> handle,
        Func<TCommand, string> getId
    ) =>
        services
            .AddScoped<Func<TCommand, CancellationToken, ValueTask>>(sp =>
            {
                var eventStore = sp.GetRequiredService<EventStoreClient>();
                var nextStreamRevisionProvider =
                    sp.GetRequiredService<INextResourceVersionProvider>();

                return async (command, ct) =>
                {
                    var nextRevision = await HandleCreateCommand(eventStore, handle(sp), getId, command, ct);
                    nextStreamRevisionProvider.TrySet(nextRevision.ToString());
                };
            });

    public static IServiceCollection AddUpdateCommandHandler<TCommand, TEntity>(
        this IServiceCollection services,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<TCommand, TEntity, object> handle,
        Func<TCommand, string> getId
    ) where TEntity : notnull =>
        services.AddUpdateCommandHandler(getDefault, when, _ => handle, getId);

    public static IServiceCollection AddUpdateCommandHandler<TCommand, TEntity>(
        this IServiceCollection services,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<IServiceProvider, Func<TCommand, TEntity, object>> handle,
        Func<TCommand, string> getId
    ) where TEntity : notnull =>
        services
            .AddScoped<Func<TCommand, CancellationToken, ValueTask>>(sp =>
            {
                var eventStoreClient = sp.GetRequiredService<EventStoreClient>();

                var expectedStreamRevisionProvider =
                    sp.GetRequiredService<IExpectedResourceVersionProvider>();
                var nextStreamRevisionProvider =
                    sp.GetRequiredService<INextResourceVersionProvider>();

                ulong GetExpectedVersion()
                {
                    var value = expectedStreamRevisionProvider.Value;

                    if (string.IsNullOrWhiteSpace(value) || !ulong.TryParse(value, out var expectedRevision))
                        throw new ArgumentNullException("ETag", "Expected version not set");

                    return expectedRevision;
                }

                return async (command, ct) =>
                {
                    var nextRevision = await HandleUpdateCommand(
                        eventStoreClient,
                        getDefault,
                        when,
                        handle(sp),
                        getId,
                        command,
                        GetExpectedVersion(),
                        ct
                    );
                    nextStreamRevisionProvider.TrySet(nextRevision.ToString());
                };
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
            services.AddCreateCommandHandler(_ => handle, getId);
            return this;
        }

        public CommandHandlersBuilder<TEntity> AddOn<TCommand>(
            Func<IServiceProvider, Func<TCommand, object>> handle,
            Func<TCommand, string> getId
        )
        {
            services.AddCreateCommandHandler(_ => handle, getId);
            return this;
        }

        public CommandHandlersBuilder<TEntity> UpdateOn<TCommand>(
            Func<TCommand, TEntity, object> handle,
            Func<TCommand, string> getId
        )
        {
            services.AddUpdateCommandHandler(getDefault, when, _ => handle, getId);
            return this;
        }

        public CommandHandlersBuilder<TEntity> UpdateOn<TCommand>(
            Func<IServiceProvider, Func<TCommand, TEntity, object>> handle,
            Func<TCommand, string> getId
        )
        {
            services.AddUpdateCommandHandler(getDefault, when, handle, getId);
            return this;
        }
    }
}
