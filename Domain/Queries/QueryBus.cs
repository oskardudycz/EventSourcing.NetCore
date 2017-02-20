using MediatR;
using System.Threading.Tasks;

namespace Domain.Queries
{
    public class QueryBus : IQueryBus
    {
        private IMediator _mediator;

        public QueryBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<TResponse> Send<TResponse>(IQuery<TResponse> command)
        {
            return _mediator.Send(command);
        }
    }
}
