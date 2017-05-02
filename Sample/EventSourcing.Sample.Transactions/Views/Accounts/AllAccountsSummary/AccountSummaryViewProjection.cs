using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using Marten;
using Marten.Events.Projections;
using System;
using System.Linq;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AccountSummaryViewProjection : ViewProjection<AccountsSummaryView>
    {
        public AccountSummaryViewProjection()
        {
            ProjectEventToSingleRecord<NewAccountCreated>(Persist);    
        }

        private void Persist(AccountsSummaryView view, NewAccountCreated @event)
        {
            view.ApplyEvent(@event);
        }
        
        ViewProjection<AccountsSummaryView> ProjectEventToSingleRecord<TEvent>(Action<AccountsSummaryView, TEvent> handler) where TEvent : class
        {
            return ProjectEvent((documentSession, ev) => FindIdOfRecord(documentSession) ?? Guid.NewGuid(), handler);
        }

        Guid? FindIdOfRecord(IDocumentSession documentSession)
        {
            return documentSession.Query<AccountsSummaryView>()
                               .Select(t => (Guid?)t.Id).SingleOrDefault();
        }
    }
}
