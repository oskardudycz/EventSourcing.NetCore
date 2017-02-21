using Marten.Events.Projections;
using System;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Accounts;
using System.Collections.Generic;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;
using System.Linq;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AccountsSummaryView
    {
        public Guid Id { get; set; }
        public IList<AccountSummary> Accounts { get; } = new List<AccountSummary>();

        enum Multiplier
        {
            Plus = 1,
            Minus = -1
        }

        public void ApplyEvent(NewAccountCreated @event)
        {
            Accounts.Add(new AccountSummary()
            {
                AccountId = @event.AccountId,
                ClientId = @event.ClientId,
                Balance = 0,
                TransactionsCount = 0
            });
        }

        public void ApplyEvent(NewInflowRecorded @event)
        {
            Apply(@event.FromAccountId, @event.Outflow.Ammount, Multiplier.Plus);
            Apply(@event.ToAccountId, @event.Outflow.Ammount, Multiplier.Minus);
        }

        public void ApplyEvent(NewOutflowRecorded @event)
        {
            Apply(@event.FromAccountId, @event.Outflow.Ammount, Multiplier.Minus);
            Apply(@event.ToAccountId, @event.Outflow.Ammount, Multiplier.Plus);
        }

        private void Apply(Guid accountId, decimal ammount, Multiplier multiplier)
        {
            var account = Accounts.FirstOrDefault(a => a.AccountId == accountId);
            account.Balance += (int)multiplier * ammount;
            account.TransactionsCount++;
        }
    }
}
