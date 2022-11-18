namespace Core.Commands.Mediator;

public interface IMediatorCommandBus
{
    Task Send<TCommand>(TCommand command) where TCommand : ICommand;
}
