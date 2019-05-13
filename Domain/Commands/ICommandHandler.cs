using MediatR;

namespace Domain.Commands
{
    public interface ICommandHandler<in T>: IRequestHandler<T>
        where T : ICommand
    {
    }
}
