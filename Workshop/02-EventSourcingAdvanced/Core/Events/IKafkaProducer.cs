using System.Threading.Tasks;

namespace Core.Events
{
    public interface IKafkaProducer
    {
        Task Publish(object @event);
    }
}