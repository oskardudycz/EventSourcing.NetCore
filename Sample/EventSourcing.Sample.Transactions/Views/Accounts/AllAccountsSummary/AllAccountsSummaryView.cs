using System;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AllAccountsSummaryView
    {
        public Guid Id { get; set; }

        public int TotalCount { get; set; }

        public decimal TotalBalance { get; set; }

        public int TotalTransactionsCount { get; set; }

        public void ApplyEvent(NewAccountCreated @event)
        {
            TotalCount++;
        }

        public void ApplyEvent(NewInflowRecorded @event)
        {
            TotalBalance += @event.Inflow.Ammount;
            TotalTransactionsCount++;
        }

        public void ApplyEvent(NewOutflowRecorded @event)
        {
            TotalBalance -= @event.Outflow.Ammount;
            TotalTransactionsCount++;
        }
    }
}
