using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using Marten;
using Marten.Events;

namespace EventSourcing.Sample.Tasks.Domain.Accounts.Handlers
{
    public class ProcessInflowHandler : ICommandHandler<ProcessInflow>
    {
        private readonly IDocumentSession _session;
        private IEventStore _store => _session.Events;
        public ProcessInflowHandler(IDocumentSession session)
        {
            _session = session;
        }

        public void Handle(ProcessInflow command)
        {
            var account = _store.AggregateStream<Account>(command.ToAccountId);

            account.RecordInflow(command.FromAccountId, command.Ammount);

            _store.Append(account.Id, account.PendingEvents.ToArray());

        }
    }
}
