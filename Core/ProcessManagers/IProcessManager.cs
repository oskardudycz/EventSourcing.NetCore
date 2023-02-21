using Core.Projections;
using Core.Structures;

namespace Core.ProcessManagers;

public interface IProcessManager: IProcessManager<Guid>
{
}

public interface IProcessManager<out T>: IProjection
{
    T Id { get; }
    int Version { get; }

    EventOrCommand[] DequeuePendingMessages();
}

public class EventOrCommand: Either<object, object>
{
    public static EventOrCommand Event(object @event) =>
        new(Maybe<object>.Of(@event), Maybe<object>.Empty);


    public static EventOrCommand Command(object @event) =>
        new(Maybe<object>.Empty, Maybe<object>.Of(@event));

    private EventOrCommand(Maybe<object> left, Maybe<object> right): base(left, right)
    {
    }
}
