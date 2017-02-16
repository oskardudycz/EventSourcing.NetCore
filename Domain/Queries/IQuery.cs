using MediatR;

namespace Domain.Queries
{
    public interface IQuery<TResponse> : IRequest<TResponse>
    {
    }
}
