using MediatR;

namespace Domain.Commands
{
    public interface ICommandHandler<T> : IRequestHandler<T> where T : ICommand
    {
    }
}
