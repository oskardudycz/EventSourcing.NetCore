using MediatR;

namespace Core.Queries
{
    public interface IQuery<out TResponse>: IRequest<TResponse>
    {
    }
}
