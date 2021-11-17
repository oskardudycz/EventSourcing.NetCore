using System.Threading.Tasks;

namespace Core.Events;

public interface IEventBus
{
    Task Publish(params IEvent[] events);
}