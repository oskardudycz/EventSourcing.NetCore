using System.Threading.Tasks;
using Core.Events.External;
using MediatR;

namespace Core.Events
{
    public class EventBus: IEventBus
    {
        private readonly IMediator mediator;
        private readonly IExternaEventProducer producer;

        public EventBus(
            IMediator mediator,
            IExternaEventProducer producer
        )
        {
            this.mediator = mediator;
            this.producer = producer;
        }

        public async Task Publish<TEvent>(params TEvent[] events) where TEvent : IEvent
        {
            foreach (var @event in events)
            {
                await mediator.Publish(@event);
                await producer.Publish(@event);
            }
        }
    }
}
