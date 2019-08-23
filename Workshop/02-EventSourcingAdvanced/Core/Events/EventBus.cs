using System.Threading.Tasks;
using MediatR;

namespace Core.Events
{
    public class EventBus: IEventBus
    {
        private readonly IMediator mediator;
        private readonly KafkaProducer producer;

        public EventBus(IMediator mediator)
        {
            this.mediator = mediator;
            producer = new KafkaProducer("localhost:9092", "meetingsmanagement");
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
