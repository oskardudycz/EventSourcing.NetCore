using System;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Events;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Events;

namespace EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary
{
    public class AccountSummaryView
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid ClientId { get; set; }
        public string Number { get; set; } = default!;
        public decimal Balance { get; set; }
        public int TransactionsCount { get; set; }

        public void Apply(NewAccountCreated @event)
        {
            Id = @event.AccountId;
            AccountId = @event.AccountId;
            Balance = 0;
            ClientId = @event.ClientId;
            Number = @event.Number;
            TransactionsCount = 0;
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
