using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventPipelines
{
    public delegate ValueTask EventDelegate(object? @event, CancellationToken ct);

    public interface IEventBus
    {
        Task Publish(object @event, CancellationToken ct);
    }

    public interface IEventHandler
    {
        Type CanHandle { get; }

        ValueTask Handle(object @event, EventDelegate onNext, CancellationToken ct);
    }

    public interface IEventHandler<in TEvent>: IEventHandler
        where TEvent: notnull
    {
        Type IEventHandler.CanHandle => typeof(TEvent);

        async ValueTask IEventHandler.Handle(object @event, EventDelegate onNext, CancellationToken ct)
        {
            await Handle((TEvent)@event, ct);
            await onNext(@event, ct);
        }

        ValueTask Handle(TEvent @event, CancellationToken ct);
    }

    public class EventHandlerWrapper<TEvent>: IEventHandler<TEvent>
        where TEvent: notnull
    {
        private readonly Func<TEvent, CancellationToken, ValueTask> handler;

        public EventHandlerWrapper(Func<TEvent, CancellationToken, ValueTask> handler)
        {
            this.handler = handler;
        }

        public ValueTask Handle(TEvent @event, CancellationToken ct) =>
            handler(@event, ct);
    }

    public interface IEventTransformation<in TEvent, TTransformedEvent> : IEventHandler
    {
        Type IEventHandler.CanHandle => typeof(TEvent);

        async ValueTask IEventHandler.Handle(object @event, EventDelegate onNext, CancellationToken ct)
        {
            var transformed = await Handle((TEvent)@event, ct);
            await onNext(transformed, ct);
        }

        ValueTask<TTransformedEvent> Handle(TEvent @event, CancellationToken ct);
    }


    public class EventTransformationWrapper<TEvent, TTransformedEvent>: IEventTransformation<TEvent, TTransformedEvent>
    {
        private readonly Func<TEvent, CancellationToken, ValueTask<TTransformedEvent>> handler;

        public EventTransformationWrapper(Func<TEvent, CancellationToken, ValueTask<TTransformedEvent>> handler)
        {
            this.handler = handler;
        }

        public ValueTask<TTransformedEvent> Handle(TEvent @event, CancellationToken ct) =>
            handler(@event, ct);
    }

    public interface IEventFilter<in TEvent> : IEventHandler
    {
        Type IEventHandler.CanHandle => typeof(TEvent);

        async ValueTask IEventHandler.Handle(object @event, EventDelegate onNext, CancellationToken ct)
        {
            var matches = await Handle((TEvent)@event, ct);

            if (!matches)
            {
                await onNext(null, ct);
            }

            await onNext(@event, ct);
        }

        ValueTask<bool> Handle(TEvent @event, CancellationToken ct);
    }

    public class EventFilterWrapper<TEvent>: IEventFilter<TEvent>
        where TEvent: notnull
    {
        private readonly Func<TEvent, CancellationToken, ValueTask<bool>> handler;

        public EventFilterWrapper(Func<TEvent, CancellationToken, ValueTask<bool>> handler)
        {
            this.handler = handler;
        }

        public ValueTask<bool> Handle(TEvent @event, CancellationToken ct) =>
            handler(@event, ct);
    }

    public class EventBus: IEventBus
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IDictionary<Type, List<IEventHandler>> handlers;

        public EventBus(
            IServiceProvider serviceProvider,
            IEnumerable<IEventHandler> handlers
        )
        {
            this.serviceProvider = serviceProvider;
            this.handlers = handlers
                .GroupBy(h => h.CanHandle)
                .ToDictionary(h => h.Key, h => h.ToList());
        }

        public Task Publish(object @event, CancellationToken ct)
        {
            var eventType = @event.GetType();

            if (!handlers.TryGetValue(eventType, out var eventHandlers))
                return;

            return Task.CompletedTask;
        }

        // {
        //     using var scope = serviceProvider.CreateScope();
        //
        //     var eventHandlers = scope.ServiceProvider
        //         .GetServices<Func<IServiceProvider, ResolvedEvent, CancellationToken, Task>>();
        //
        //     foreach (var handle in eventHandlers)
        //     {
        //         await retryPolicy.ExecuteAsync(async token =>
        //         {
        //             await handle(scope.ServiceProvider, @event, token);
        //         }, ct);
        //     }
        }
    }
}
