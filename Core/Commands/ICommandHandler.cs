namespace Core.Commands;

public interface ICommandHandler<in TCommand>
{
    Task Handle(TCommand request, CancellationToken cancellationToken);
}
