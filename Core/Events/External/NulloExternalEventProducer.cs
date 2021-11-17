using System.Threading.Tasks;

namespace Core.Events.External;

public class NulloExternalEventProducer : IExternalEventProducer
{
    public Task Publish(IExternalEvent @event)
    {
        return Task.CompletedTask;
    }
}