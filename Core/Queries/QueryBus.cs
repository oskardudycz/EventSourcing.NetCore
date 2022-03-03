using MediatR;

namespace Core.Queries;

public class QueryBus: IQueryBus
{
    private readonly IMediator mediator;

    public QueryBus(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public Task<TResponse> Send<TQuery, TResponse>(TQuery query) where TQuery : IQuery<TResponse>
    {
        return mediator.Send(query);
    }
}