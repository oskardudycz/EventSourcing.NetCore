using MediatR;

namespace Core.Commands.Mediator;

public class MediatorCommandBus: IMediatorCommandBus
{
    private readonly IMediator mediator;

    public MediatorCommandBus(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public Task Send<TCommand>(TCommand command) where TCommand : ICommand
    {
        return mediator.Send(command);
    }
}
