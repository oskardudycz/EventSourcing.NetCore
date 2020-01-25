using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;

namespace Core.Events
{
    public class EventBus: IEventBus
    {
        private readonly IMediator mediator;
        //private readonly IExternalEventProducer producer;

        public EventBus(
            IMediator mediator//,
            //IExternalEventProducer producer
        )
        {
            Guard.Against.Null(mediator, nameof(Mediator));
            this.mediator = mediator;
            //this.producer = producer;
        }

        public async Task Publish(params IEvent[] events)
        {
            foreach (var @event in events)
            {
                await mediator.Publish(@event);

                // if (@event is IExternalEvent externalEvent)
                //     await producer.Publish(externalEvent);
            }
        }
    }
}
