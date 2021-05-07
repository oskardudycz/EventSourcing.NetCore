using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Events;
using Core.Exceptions;
using Core.Marten.Aggregates;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Commands;
using EventSourcing.Sample.Transactions.Views.Clients;
using Marten;
using Marten.Events;
using Marten.Util;
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

        private IEventStore Store => session.Events;

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
            var (fromAccountId, toAccountId, amount) = command;
            var accountFrom = await Store.AggregateStreamAsync<Account>(fromAccountId, token: cancellationToken)
                              ?? throw AggregateNotFoundException.For<Account>(fromAccountId);

            accountFrom.RecordOutflow(toAccountId, amount);

            var accountFromEvents = accountFrom.DequeueUncommittedEvents();
            Store.Append(accountFrom.Id, accountFromEvents);

            var accountTo = await Store.AggregateStreamAsync<Account>(toAccountId, token: cancellationToken)
                            ?? throw AggregateNotFoundException.For<Account>(toAccountId);

            accountTo.RecordInflow(fromAccountId, amount);

            var accountToEvents = accountFrom.DequeueUncommittedEvents();
            Store.Append(accountTo.Id, accountTo);

            await session.SaveChangesAsync(cancellationToken);

            await eventBus.Publish(accountFromEvents);
            await eventBus.Publish(accountToEvents);

            return Unit.Value;
        }
    }
}
