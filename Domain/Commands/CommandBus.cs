using MediatR;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class CommandBus : ICommandBus
    {
        private IMediator _mediator;

        internal CommandBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Send(ICommand command)
        {
            return _mediator.Send(command);
        }
    }
}
