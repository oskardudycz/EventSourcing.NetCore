using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using Marten;
using Marten.Events.Projections;
using System;
using System.Linq;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AllAccountsSummaryViewProjection : ViewProjection<AllAccountsSummaryView>
    {
        public AllAccountsSummaryViewProjection()
        {
            ProjectEventToSingleRecord<NewAccountCreated>(Persist);
            ProjectEventToSingleRecord<NewInflowRecorded>(Persist);
            ProjectEventToSingleRecord<NewOutflowRecorded>(Persist);
        }

        private void Persist(AllAccountsSummaryView view, NewAccountCreated @event)
        {
            view.ApplyEvent(@event);
        }

        private void Persist(AllAccountsSummaryView view, NewInflowRecorded @event)
        {
            view.ApplyEvent(@event);
        }

        private void Persist(AllAccountsSummaryView view, NewOutflowRecorded @event)
        {
            view.ApplyEvent(@event);
        }

        ViewProjection<AllAccountsSummaryView> ProjectEventToSingleRecord<TEvent>(Action<AllAccountsSummaryView, TEvent> handler) where TEvent : class
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
