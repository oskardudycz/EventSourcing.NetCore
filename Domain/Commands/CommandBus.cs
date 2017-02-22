using MediatR;
using System.Threading.Tasks;

namespace Domain.Commands
{
    public class CommandBus : ICommandBus
    {
        private readonly IMediator _mediator;

        public CommandBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Send(ICommand command)
        {
            return _mediator.Send(command);
        }
    }
}
