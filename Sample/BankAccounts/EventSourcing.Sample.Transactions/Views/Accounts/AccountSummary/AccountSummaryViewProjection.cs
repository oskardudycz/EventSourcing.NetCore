using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;
using Marten;
using Marten.Events.Projections;

namespace EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary
{
    public class AccountSummaryViewProjection: ViewProjection<AccountSummaryView, Guid>
    {
        public AccountSummaryViewProjection()
        {
            ProjectEvent<NewAccountCreated>((ev) => ev.AccountId, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent<NewInflowRecorded>((ev) => ev.ToAccountId, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent<NewOutflowRecorded>((ev) => ev.FromAccountId, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent((IDocumentSession session, ClientCreated @event) => FindClientAccountIds(session, @event), (view, @event) => view.ApplyEvent(@event));
            ProjectEvent((IDocumentSession session, ClientUpdated @event) => FindClientAccountIds(session, @event), (view, @event) => view.ApplyEvent(@event));
            ProjectEvent((IDocumentSession session, ClientDeleted @event) => FindClientAccountIds(session, @event), (view, @event) => view.ApplyEvent(@event));
        }

        private List<Guid> FindClientAccountIds(IDocumentSession session, ClientCreated @event)
        {
            return FindClientAccountIds(session, @event.ClientId);
        }

        private List<Guid> FindClientAccountIds(IDocumentSession session, ClientUpdated @event)
        {
            return FindClientAccountIds(session, @event.ClientId);
        }

        private List<Guid> FindClientAccountIds(IDocumentSession session, ClientDeleted @event)
        {
            return FindClientAccountIds(session, @event.ClientId);
        }

        private List<Guid> FindClientAccountIds(IDocumentSession session, Guid clientId)
        {
            return session.Query<AccountSummaryView>()
                .Where(a => a.ClientId == clientId)
                .Select(a => a.AccountId)
                .ToList();
        }
    }
}
