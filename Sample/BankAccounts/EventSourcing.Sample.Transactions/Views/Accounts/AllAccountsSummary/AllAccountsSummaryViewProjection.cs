using System;
using System.Linq;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Events;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Events;
using Marten;
using Marten.Events.Projections;

namespace EventSourcing.Sample.Transactions.Views.Accounts.AllAccountsSummary
{
    public class AllAccountsSummaryViewProjection: EventProjection
    {
        public void Project(NewAccountCreated @event, IDocumentOperations operations)
        {
            var issue = operations.Load<AllAccountsSummaryView>(Guid.Empty) ?? new AllAccountsSummaryView();
            issue.Apply(@event);
            operations.Store(issue);
        }

        public void Project(NewInflowRecorded @event, IDocumentOperations operations)
        {
            var issue = operations.Load<AllAccountsSummaryView>(Guid.Empty) ?? new AllAccountsSummaryView();
            issue.Apply(@event);
            operations.Store(issue);
        }

        public void Project(NewOutflowRecorded @event, IDocumentOperations operations)
        {
            var issue = operations.Load<AllAccountsSummaryView>(Guid.Empty) ?? new AllAccountsSummaryView();
            issue.Apply(@event);
            operations.Store(issue);
        }
    }
}
