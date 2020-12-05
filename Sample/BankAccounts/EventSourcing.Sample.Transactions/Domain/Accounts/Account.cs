using System;
using Core.Aggregates;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Events;
using EventSourcing.Sample.Transactions.Contracts.Transactions;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Events;

namespace EventSourcing.Sample.Transactions.Domain.Accounts
{
    public class Account: Aggregate
    {
        public Guid ClientId { get; private set; }

        public decimal Balance { get; private set; }

        public string Number { get; private set; }

        public Account()
        {
        }

        public Account(Guid clientId, IAccountNumberGenerator accountNumberGenerator)
        {
            var @event = new NewAccountCreated
            {
                AccountId = Guid.NewGuid(),
                ClientId = clientId,
                Number = accountNumberGenerator.Generate()
            };

            Apply(@event);
            Enqueue(@event);
        }

        public void RecordInflow(Guid fromId, decimal amount)
        {
            var @event = new NewInflowRecorded(fromId, Id, new Inflow(amount, DateTime.Now));

            Apply(@event);
            Enqueue(@event);
        }

        public void RecordOutflow(Guid toId, decimal amount)
        {
            var @event = new NewOutflowRecorded(Id, toId, new Outflow(amount, DateTime.Now));

            Apply(@event);
            Enqueue(@event);
        }

        public void Apply(NewAccountCreated @event)
        {
            Id = @event.AccountId;
            ClientId = @event.ClientId;
            Number = @event.Number;
        }

        public void Apply(NewInflowRecorded @event)
        {
            Balance += @event.Inflow.Amount;
        }

        public void Apply(NewOutflowRecorded @event)
        {
            Balance -= @event.Outflow.Amount;
        }
    }
}
