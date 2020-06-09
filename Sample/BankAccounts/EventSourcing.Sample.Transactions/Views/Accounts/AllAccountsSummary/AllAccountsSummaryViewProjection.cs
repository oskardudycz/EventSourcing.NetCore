using System;
using System.Linq;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;
using Marten;
using Marten.Events.Projections;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AllAccountsSummaryViewProjection: ViewProjection<AllAccountsSummaryView, Guid>
    {
        public AllAccountsSummaryViewProjection()
        {
            ProjectEventToSingleRecord<NewAccountCreated>((view, @event) => view.ApplyEvent(@event));
            ProjectEventToSingleRecord<NewInflowRecorded>((view, @event) => view.ApplyEvent(@event));
            ProjectEventToSingleRecord<NewOutflowRecorded>((view, @event) => view.ApplyEvent(@event));
        }

        private ViewProjection<AllAccountsSummaryView, Guid> ProjectEventToSingleRecord<TEvent>(Action<AllAccountsSummaryView, TEvent> handler) where TEvent : class
        {
            return ProjectEvent((documentSession, ev) => FindIdOfRecord(documentSession) ?? Guid.NewGuid(), handler);
        }

        private Guid? FindIdOfRecord(IDocumentSession documentSession)
        {
            return documentSession.Query<AllAccountsSummaryView>()
                               .Select(t => (Guid?)t.Id).SingleOrDefault();
        }
    }
}
