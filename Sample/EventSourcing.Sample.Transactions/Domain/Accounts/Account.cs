using Domain.Aggregates;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Transactions;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;
using System;

namespace EventSourcing.Sample.Tasks.Domain.Accounts
{
    public class Account : EventSource
    {
        public Account()
        {
        }

        public Account(Guid clientId)
        {
            var @event = new NewAccountCreated
            {
                AccountId = Guid.NewGuid(),
                ClientId = clientId
            };

            Apply(@event);
            Append(@event);
        }

        public Guid ClientId { get; private set; }

        public decimal Balance { get; private set; }

        public void RecordInflow(Guid fromId, decimal ammount)
        {
            var @event = new NewInflowRecorded(fromId, Id, new Inflow(ammount, DateTime.Now));
            Apply(@event);
            Append(@event);
        }

        public void RecordOutflow(Guid toId, decimal ammount)
        {
            var @event = new NewOutflowRecorded(Id, toId, new Outflow(ammount, DateTime.Now));
            Apply(@event);
            Append(@event);
        }

        private void Apply(NewAccountCreated @event)
        {
            Id = @event.AccountId;
            ClientId = @event.ClientId;
        }

        public void Apply(NewInflowRecorded @event)
        {
            Balance += @event.Outflow.Ammount;
        }

        public void Apply(NewOutflowRecorded @event)
        {
            Balance -= @event.Outflow.Ammount;
        }
    }
}
