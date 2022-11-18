namespace Core.Commands;

public interface ICommandBus
{
    Task Send<TCommand>(TCommand command, CancellationToken ct = default) where TCommand: notnull;
}
