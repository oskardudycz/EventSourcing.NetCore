using System.Threading.Tasks;

namespace Core.Events.External
{
    public interface IExternaEventProducer
    {
        Task Publish(object @event);
    }
}
