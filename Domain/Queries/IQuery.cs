using MediatR;

namespace Domain.Queries
{
    public interface IQuery<out TResponse>: IRequest<TResponse>
    {
    }
}
