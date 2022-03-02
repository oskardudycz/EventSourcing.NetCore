using System.Threading;
using System.Threading.Tasks;

namespace Core.Events.NoMediator;

public interface INoMediatorEventHandler<in TEvent>
{
    Task Handle(TEvent @event, CancellationToken ct);
}
