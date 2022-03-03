namespace EventStoreBasics;

public interface IProjection
{
    Type[] Handles { get; }

    void Handle(object @event);
}

public abstract class Projection: IProjection
{
    public Type[] Handles { get; set; } = default!;

    protected void Projects<TEvent>(Action<TEvent> action)
    {
        throw new NotImplementedException("TODO add storing the projection logic.");
    }

    public void Handle(object @event)
    {
        throw new NotImplementedException("TODO add event handling.");
    }
}