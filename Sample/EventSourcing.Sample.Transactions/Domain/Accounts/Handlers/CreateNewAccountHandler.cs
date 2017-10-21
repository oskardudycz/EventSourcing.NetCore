using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Domain.Accounts;
using EventSourcing.Sample.Transactions.Views.Clients;
using Marten;
using Marten.Events;
using System;
using System.Linq;

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
            if (!_session.Query<ClientsView>().Any(c => c.Id == command.ClientId))
                throw new ArgumentException("Client does not exist!", nameof(command.ClientId));

            var account = new Account(command.ClientId, _accountNumberGenerator);

            _store.Append(account.Id, account.PendingEvents.ToArray());
            _session.SaveChanges();
        }
    }
}
