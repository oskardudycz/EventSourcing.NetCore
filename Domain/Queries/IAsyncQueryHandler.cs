using MediatR;


namespace Domain.Queries
{
    interface IAsyncQueryHandler<in TQuery, TResponse> : IAsyncRequestHandler<TQuery, TResponse>
           where TQuery : IQuery<TResponse>
    {
    }
}
