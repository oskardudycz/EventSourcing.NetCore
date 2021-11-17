using System.Threading;
using System.Threading.Tasks;

namespace Core.Events.External;

public interface IExternalEventConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}