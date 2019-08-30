using System.Threading.Tasks;
using Core.Events;

namespace Core.Testing
{
    public class DummyInternalEventBus: IEventBus
    {
        private readonly EventBus eventBus;

        public DummyInternalEventBus(EventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public Task Publish(params IEvent[] events)
        {
            return eventBus.Publish(events);
        }
    }
}
