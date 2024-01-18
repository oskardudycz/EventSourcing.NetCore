namespace Core.Structures;

public class EventOrCommand: Either<object, object>
{
    public static EventOrCommand Event(object @event) =>
        new(Maybe<object>.Of(@event), Maybe<object>.Empty);

    public static IEnumerable<EventOrCommand> Events(params object[] events) =>
        events.Select(Event);

    public static IEnumerable<EventOrCommand> Events(IEnumerable<object> events) =>
        events.Select(Event);

    public static EventOrCommand Command(object @event) =>
        new(Maybe<object>.Empty, Maybe<object>.Of(@event));

    private EventOrCommand(Maybe<object> left, Maybe<object> right): base(left, right)
    {
    }
}
