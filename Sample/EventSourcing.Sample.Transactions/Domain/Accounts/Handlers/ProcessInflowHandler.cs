using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using Marten;
using Marten.Events;
using MediatR;

namespace EventSourcing.Sample.Tasks.Domain.Accounts.Handlers
{
    public class ProcessInflowHandler : ICommandHandler<MakeTransfer>
    {
        private readonly IDocumentSession _session;
        private IEventStore _store => _session.Events;

        public ProcessInflowHandler(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<Unit> Handle(MakeTransfer command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var accountFrom = await _store.AggregateStreamAsync<Account>(command.FromAccountId, token: cancellationToken);

            accountFrom.RecordOutflow(command.ToAccountId, command.Ammount);
            _store.Append(accountFrom.Id, accountFrom.PendingEvents.ToArray());

            var accountTo = await _store.AggregateStreamAsync<Account>(command.ToAccountId, token: cancellationToken);

            accountTo.RecordInflow(command.FromAccountId, command.Ammount);
            _store.Append(accountTo.Id, accountTo.PendingEvents.ToArray());

            await _session.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}