using MediatR;

namespace Domain.Queries
{
    public interface IQueryHandler<in TQuery, out TResponse> : IRequestHandler<TQuery, TResponse>
           where TQuery : IQuery<TResponse>
    {
    }
}
