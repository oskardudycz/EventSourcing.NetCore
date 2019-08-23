using System.Threading.Tasks;
using MediatR;

namespace Core.Events
{
    public class EventBus: IEventBus
    {
        private readonly IMediator mediator;
        private readonly IKafkaProducer producer;

        public EventBus(
            IMediator mediator,
            IKafkaProducer producer
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
