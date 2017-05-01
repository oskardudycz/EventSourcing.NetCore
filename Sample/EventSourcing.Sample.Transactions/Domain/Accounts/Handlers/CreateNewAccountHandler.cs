using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Domain.Accounts;
using Marten;
using Marten.Events;

namespace EventSourcing.Sample.Tasks.Domain.Accounts.Handlers
{
    public class CreateNewAccountHandler : ICommandHandler<CreateNewAccount>
    {
        private readonly IDocumentSession _session;
        private readonly IAccountNumberGenerator _accountNumberGenerator;

        private IEventStore _store => _session.Events;
        public CreateNewAccountHandler(IDocumentSession session, IAccountNumberGenerator accountNumberGenerator)
        {
            _session = session;
            _accountNumberGenerator = accountNumberGenerator;
        }

        public void Handle(CreateNewAccount command)
        {
            var account = new Account(command.ClientId, _accountNumberGenerator);

            _store.Append(account.Id, account.PendingEvents.ToArray());
            _session.SaveChanges();
        }
    }
}
