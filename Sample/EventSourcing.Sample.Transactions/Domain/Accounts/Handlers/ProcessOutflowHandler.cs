using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using Marten;
using Marten.Events;

namespace EventSourcing.Sample.Tasks.Domain.Accounts.Handlers
{
    public class ProcessOutflowHandler : ICommandHandler<ProcessOutflow>
    {
        private readonly IDocumentSession _session;
        private IEventStore _store => _session.Events;
        public ProcessOutflowHandler(IDocumentSession session)
        {
            _session = session;
        }

        public void Handle(ProcessOutflow command)
        {
            var account = _store.AggregateStream<Account>(command.FromAccountId);

            account.RecordOutflow(command.ToAccountId, command.Ammount);

            _store.Append(account.Id, account.PendingEvents.ToArray());

        }
    }
}
