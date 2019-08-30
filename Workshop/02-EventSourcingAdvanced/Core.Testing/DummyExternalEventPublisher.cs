using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;
using Core.Events.External;

namespace Core.Testing
{
    public class DummyExternalEventProducer: IExternalEventProducer
    {
        public IList<IExternalEvent> PublishedEvents { get; } = new List<IExternalEvent>();

        public Task Publish(IExternalEvent @event)
        {
            PublishedEvents.Add(@event);

            return Task.CompletedTask;
        }
    }
}
