using MediatR;


namespace Domain.Queries
{
    interface IAsyncQueryHandler<TQuery, TResponse> : IAsyncRequestHandler<TQuery, TResponse>
           where TQuery : IQuery<TResponse>
    {
    }
}
