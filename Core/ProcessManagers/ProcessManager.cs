namespace Core.ProcessManagers;

public abstract class ProcessManager: ProcessManager<Guid>, IProcessManager;

public abstract class ProcessManager<T>: IProcessManager<T> where T : notnull
{
    public T Id { get; protected set; } = default!;

    public int Version { get; protected set; }

    [NonSerialized] private readonly Queue<EventOrCommand> scheduledCommands = new();

    public EventOrCommand[] DequeuePendingMessages()
    {
        var dequeuedEvents = scheduledCommands.ToArray();

        scheduledCommands.Clear();

        return dequeuedEvents;
    }

    protected void EnqueueEvent(object @event) =>
        scheduledCommands.Enqueue(EventOrCommand.Event(@event));

    protected void ScheduleCommand(object @event) =>
        scheduledCommands.Enqueue(EventOrCommand.Command(@event));

    public virtual void Apply(object @event)
    {
    }
}
