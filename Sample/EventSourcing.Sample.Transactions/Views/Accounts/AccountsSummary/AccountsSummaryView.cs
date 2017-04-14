using System;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AccountsSummaryView
    {
        public Guid Id { get; private set; }

        public int TotalCount { get; private set; }
        
        public decimal TotalBalance { get; private set; }

        public int TotalTransactionsCount{ get; private set; }

        public void Apply(NewAccountCreated @event)
        {
            TotalCount++;
        }

        public void Apply(NewInflowRecorded @event)
        {
            TotalBalance += @event.Inflow.Ammount;
            TotalTransactionsCount++;
        }

        public void Apply(NewOutflowRecorded @event)
        {
            TotalBalance += @event.Outflow.Ammount;
            TotalTransactionsCount++;
        }
    }
}
