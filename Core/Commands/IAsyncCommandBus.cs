namespace Core.Commands;

public interface IAsyncCommandBus
{
    Task Schedule<TCommand>(TCommand command, CancellationToken ct = default) where TCommand: notnull;
}

