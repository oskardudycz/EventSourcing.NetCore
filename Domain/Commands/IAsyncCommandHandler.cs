using MediatR;

namespace Domain.Commands
{
    public interface IAsyncCommandHandler<T> : IAsyncRequestHandler<T> where T : ICommand
    {
    }
}
