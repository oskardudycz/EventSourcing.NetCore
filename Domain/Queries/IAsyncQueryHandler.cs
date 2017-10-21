using MediatR;

namespace Domain.Queries
{
    public interface IAsyncQueryHandler<in TQuery, TResponse> : IAsyncRequestHandler<TQuery, TResponse>
           where TQuery : IQuery<TResponse>
    {
    }
}
