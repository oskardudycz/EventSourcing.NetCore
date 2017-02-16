using MediatR;

namespace Domain.Queries
{
    interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
           where TQuery : IQuery<TResponse>
    {
    }
}
