using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Events;
using Core.Marten.Aggregates;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Commands;
using EventSourcing.Sample.Transactions.Views.Clients;
using Marten;
using Marten.Events;
using MediatR;

namespace EventSourcing.Sample.Transactions.Domain.Accounts.Handlers
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

        public async Task<Unit> Handle(CreateNewAccount command, CancellationToken cancellationToken = default)
        {
            if (!session.Query<ClientView>().Any(c => c.Id == command.ClientId))
                throw new ArgumentException("Client does not exist!", nameof(command.ClientId));

            var account = new Account(command.ClientId, accountNumberGenerator);

            await account.StoreAndPublishEvents(session, eventBus, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(MakeTransfer command, CancellationToken cancellationToken = default)
        {
            var accountFrom = await store.AggregateStreamAsync<Account>(command.FromAccountId, token: cancellationToken);

            accountFrom.RecordOutflow(command.ToAccountId, command.Amount);

            var accountFromEvents = accountFrom.DequeueUncommittedEvents();
            store.Append(accountFrom.Id, accountFromEvents);

            var accountTo = await store.AggregateStreamAsync<Account>(command.ToAccountId, token: cancellationToken);

            accountTo.RecordInflow(command.FromAccountId, command.Amount);

            var accountToEvents = accountFrom.DequeueUncommittedEvents();
            store.Append(accountTo.Id, accountTo);

            await session.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(accountFromEvents);
            await eventBus.Publish(accountToEvents);

            return Unit.Value;
        }
    }
}
