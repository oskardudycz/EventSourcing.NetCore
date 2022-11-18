using MediatR;

namespace Core.Commands.Mediator;

public interface IMediatorCommandHandler<in T>: IRequestHandler<T>
    where T : ICommand
{
}
