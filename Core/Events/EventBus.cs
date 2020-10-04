using System;
using System.Threading.Tasks;
using Core.Events.External;
using MediatR;

namespace Core.Events
{
    public class EventBus: IEventBus
    {
        private readonly IMediator mediator;
        private readonly IExternalEventProducer externalEventProducer;

        public EventBus(
            IMediator mediator,
            IExternalEventProducer externalEventProducer
        )
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.externalEventProducer = externalEventProducer?? throw new ArgumentNullException(nameof(externalEventProducer));
        }

        public async Task Publish(params IEvent[] events)
        {
            foreach (var @event in events)
            {
                await mediator.Publish(@event);

                if (@event is IExternalEvent externalEvent)
                    await externalEventProducer.Publish(externalEvent);
            }
        }
    }
}
