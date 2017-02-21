using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using Marten;
using Marten.Events;

namespace EventSourcing.Sample.Tasks.Domain.Accounts.Handlers
{
    public class CreateNewAccountHandler : ICommandHandler<CreateNewAccount>
    {
        private readonly IDocumentSession _session;
        private IEventStore _store => _session.Events;
        public CreateNewAccountHandler(IDocumentSession session)
        {
            _session = session;
        }

        public void Handle(CreateNewAccount command)
        {
            var account = new Account(command.ClientId);

            _store.StartStream<Account>(account.PendingEvents.ToArray());
            _session.SaveChanges();
        }
    }
}
