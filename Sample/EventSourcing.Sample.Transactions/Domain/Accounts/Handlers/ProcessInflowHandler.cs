using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using Marten;
using Marten.Events;

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

        public void Handle(MakeTransfer command)
        {
            var accountFrom = _store.AggregateStream<Account>(command.FromAccountId);

            accountFrom.RecordOutflow(command.ToAccountId, command.Ammount);
            _store.Append(accountFrom.Id, accountFrom.PendingEvents.ToArray());


            var accountTo = _store.AggregateStream<Account>(command.ToAccountId);

            accountTo.RecordInflow(command.FromAccountId, command.Ammount);
            _store.Append(accountTo.Id, accountTo.PendingEvents.ToArray());

            _session.SaveChanges();
        }
    }
}
