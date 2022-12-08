namespace Core.Commands;

public interface ICommandHandler<in TCommand>
{
    Task Handle(TCommand command, CancellationToken ct);
}
