using Core.EventStoreDB.OptimisticConcurrency;
using Core.Tracing;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
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
        TraceMetadata traceMetadata,
        CancellationToken ct)
    {
        var entityId = getId(command);
        var @event = handle(command);

        return eventStore.Append(entityId, @event, traceMetadata, ct);
    }

    public static async Task<ulong> HandleUpdateCommand<TCommand, TEntity>(
        EventStoreClient eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<TCommand, TEntity, object> handle,
        Func<TCommand, string> getId,
        TCommand command,
        ulong expectedVersion,
        TraceMetadata traceMetadata,
        CancellationToken ct) where TEntity : notnull
    {
        var id = getId(command);
        var entity = await eventStore.Find(getDefault, when, id, ct);

        var @event = handle(command, entity);

        return await eventStore.Append(id, @event, expectedVersion, traceMetadata, ct);
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
        Func<TCommand, string> getId,
        Action<ulong>? setNextExpectedVersion = null
    ) =>
        services
            .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
            {
                var eventStore = sp.GetRequiredService<EventStoreClient>();

                setNextExpectedVersion ??= sp.GetRequiredService<EventStoreDBNextStreamRevisionProvider>().Set;

                var traceMetadata = new TraceMetadata(
                    sp.GetRequiredService<ICorrelationIdProvider>().Get(),
                    sp.GetRequiredService<ICausationIdProvider>().Get()
                );

                return async (command, ct) =>
                    setNextExpectedVersion(await HandleCreateCommand(eventStore, handle(sp), getId, command, traceMetadata, ct));
            });

    public static IServiceCollection AddUpdateCommandHandler<TCommand, TEntity>(
        this IServiceCollection services,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<TCommand, TEntity, object> handle,
        Func<TCommand, string> getId,
        Func<ulong>? getExpectedVersion = null,
        Action<ulong>? setNextExpectedVersion = null
    ) where TEntity : notnull =>
        services.AddUpdateCommandHandler(getDefault, when, _ => handle, getId, getExpectedVersion,
            setNextExpectedVersion);

    public static IServiceCollection AddUpdateCommandHandler<TCommand, TEntity>(
        this IServiceCollection services,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        Func<IServiceProvider, Func<TCommand, TEntity, object>> handle,
        Func<TCommand, string> getId,
        Func<ulong>? getExpectedVersion = null,
        Action<ulong>? setNextExpectedVersion = null
    ) where TEntity : notnull =>
        services
            .AddTransient<Func<TCommand, CancellationToken, ValueTask>>(sp =>
            {
                var repository = sp.GetRequiredService<EventStoreClient>();

                getExpectedVersion ??= () =>
                    sp.GetRequiredService<EventStoreDBExpectedStreamRevisionProvider>().Value ??
                    throw new ArgumentNullException("ETag", "Expected version not set");

                setNextExpectedVersion ??=
                    sp.GetRequiredService<EventStoreDBNextStreamRevisionProvider>().Set;

                var traceMetadata = new TraceMetadata(
                    sp.GetRequiredService<ICorrelationIdProvider>().Get(),
                    sp.GetRequiredService<ICausationIdProvider>().Get()
                );
                return async (command, ct) =>
                    setNextExpectedVersion(
                        await HandleUpdateCommand(
                            repository,
                            getDefault,
                            when,
                            handle(sp),
                            getId,
                            command,
                            getExpectedVersion(),
                            traceMetadata,
                            ct
                        )
                    );
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
            services.AddCreateCommandHandler<TCommand>(_ => handle, getId);
            return this;
        }

        public CommandHandlersBuilder<TEntity> AddOn<TCommand>(
            Func<IServiceProvider, Func<TCommand, object>> handle,
            Func<TCommand, string> getId
        )
        {
            services.AddCreateCommandHandler<TCommand>(_ => handle, getId);
            return this;
        }

        public CommandHandlersBuilder<TEntity> UpdateOn<TCommand>(
            Func<TCommand, TEntity, object> handle,
            Func<TCommand, string> getId,
            Func<ulong>? getExpectedVersion = null)
        {
            services.AddUpdateCommandHandler(getDefault, when, _ => handle, getId, getExpectedVersion);
            return this;
        }

        public CommandHandlersBuilder<TEntity> UpdateOn<TCommand>(
            Func<IServiceProvider, Func<TCommand, TEntity, object>> handle,
            Func<TCommand, string> getId,
            Func<ulong>? getExpectedVersion = null
        )
        {
            services.AddUpdateCommandHandler(getDefault, when, handle, getId, getExpectedVersion);
            return this;
        }
    }
}
