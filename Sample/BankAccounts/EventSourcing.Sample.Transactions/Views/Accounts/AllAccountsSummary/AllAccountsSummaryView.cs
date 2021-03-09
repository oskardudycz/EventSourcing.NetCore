using System;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Events;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Events;

namespace EventSourcing.Sample.Transactions.Views.Accounts.AllAccountsSummary
{
    public class AllAccountsSummaryView
    {
        public Guid Id { get; set; }

        public int TotalCount { get; set; }

        public decimal TotalBalance { get; set; }

        public int TotalTransactionsCount { get; set; }

        public void Apply(NewAccountCreated @event)
        {
            TotalCount++;
        }

        public void Apply(NewInflowRecorded @event)
        {
            TotalBalance += @event.Inflow.Amount;
            TotalTransactionsCount++;
        }

        public void Apply(NewOutflowRecorded @event)
        {
            TotalBalance -= @event.Outflow.Amount;
            TotalTransactionsCount++;
        }
    }
}
