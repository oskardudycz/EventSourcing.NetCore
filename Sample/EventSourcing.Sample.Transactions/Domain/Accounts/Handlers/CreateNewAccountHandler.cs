using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Domain.Accounts;
using EventSourcing.Sample.Transactions.Views.Clients;
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

        public Task Handle(CreateNewAccount command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_session.Query<ClientView>().Any(c => c.Id == command.ClientId))
                throw new ArgumentException("Client does not exist!", nameof(command.ClientId));

            var account = new Account(command.ClientId, _accountNumberGenerator);

            _store.Append(account.Id, account.PendingEvents.ToArray());
            return _session.SaveChangesAsync(cancellationToken);
        }
    }
}