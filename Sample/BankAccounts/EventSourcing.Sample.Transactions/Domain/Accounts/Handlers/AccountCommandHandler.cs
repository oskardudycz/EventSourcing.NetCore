using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Events;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Domain.Accounts;
using EventSourcing.Sample.Transactions.Views.Clients;
using Marten;
using Marten.Events;
using MediatR;

namespace EventSourcing.Sample.Tasks.Domain.Accounts.Handlers
{
    public class AccountCommandHandler
        : ICommandHandler<CreateNewAccount>,
          ICommandHandler<MakeTransfer>
    {
        private readonly IDocumentSession session;
        private readonly IAccountNumberGenerator accountNumberGenerator;
        private readonly IEventBus eventBus;

        private IEventStore store => session.Events;

        public AccountCommandHandler(
            IDocumentSession session,
            IAccountNumberGenerator accountNumberGenerator,
            IEventBus eventBus)
        {
            this.session = session;
            this.accountNumberGenerator = accountNumberGenerator;
            this.eventBus = eventBus;
        }

        public async Task<Unit> Handle(CreateNewAccount command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!session.Query<ClientView>().Any(c => c.Id == command.ClientId))
                throw new ArgumentException("Client does not exist!", nameof(command.ClientId));

            var account = new Account(command.ClientId, accountNumberGenerator);

            store.Append(account.Id, account.PendingEvents.ToArray());
            await session.SaveChangesAsync(cancellationToken);
            await eventBus.Publish(account.PendingEvents.ToArray());

            return Unit.Value;
        }

        public async Task<Unit> Handle(MakeTransfer command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var accountFrom = await store.AggregateStreamAsync<Account>(command.FromAccountId, token: cancellationToken);

            accountFrom.RecordOutflow(command.ToAccountId, command.Ammount);
            store.Append(accountFrom.Id, accountFrom.PendingEvents.ToArray());

            var accountTo = await store.AggregateStreamAsync<Account>(command.ToAccountId, token: cancellationToken);

            accountTo.RecordInflow(command.FromAccountId, command.Ammount);
            store.Append(accountTo.Id, accountTo.PendingEvents.ToArray());

            await session.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(accountFrom.PendingEvents.ToArray());
            await eventBus.Publish(accountTo.PendingEvents.ToArray());

            return Unit.Value;
        }
    }
}
