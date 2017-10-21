using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using Marten;
using Marten.Events.Projections;
using System;
using System.Linq;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AllAccountsSummaryViewProjection : ViewProjection<AllAccountsSummaryView, Guid>
    {
        public AllAccountsSummaryViewProjection()
        {
            ProjectEventToSingleRecord<NewAccountCreated>((view, @event) => view.ApplyEvent(@event));    
            ProjectEventToSingleRecord<NewInflowRecorded>((view, @event) => view.ApplyEvent(@event));
            ProjectEventToSingleRecord<NewOutflowRecorded>((view, @event) => view.ApplyEvent(@event));
        }

        ViewProjection<AllAccountsSummaryView, Guid> ProjectEventToSingleRecord<TEvent>(Action<AllAccountsSummaryView, TEvent> handler) where TEvent : class
        {
            return ProjectEvent((documentSession, ev) => FindIdOfRecord(documentSession) ?? Guid.NewGuid(), handler);
        }

        Guid? FindIdOfRecord(IDocumentSession documentSession)
        {
            return documentSession.Query<AllAccountsSummaryView>()
                               .Select(t => (Guid?)t.Id).SingleOrDefault();
        }
    }
}
