using Core.Projections;
using Core.Structures;

namespace Core.ProcessManagers;

public interface IProcessManager: IProcessManager<Guid>;

public interface IProcessManager<out T>: IProjection
{
    T Id { get; }
    int Version { get; }

    EventOrCommand[] DequeuePendingMessages();
}
