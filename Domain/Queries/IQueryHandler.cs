using MediatR;

namespace Domain.Queries
{
    public interface IQueryHandler<in TQuery, TResponse>: IRequestHandler<TQuery, TResponse>
           where TQuery : IQuery<TResponse>
    {
    }
}
