using MediatR;

namespace Domain.Commands
{
    public interface IAsyncCommandHandler<in T> : IAsyncRequestHandler<T>
        where T : ICommand
    {
    }
}
