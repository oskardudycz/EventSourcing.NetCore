using System.Threading.Tasks;

namespace Core.Events.External;

public interface IExternalEventProducer
{
    Task Publish(IExternalEvent @event);
}