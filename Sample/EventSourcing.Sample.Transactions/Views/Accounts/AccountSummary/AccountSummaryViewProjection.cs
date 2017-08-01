using System;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;
using Marten.Events.Projections;

namespace EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary
{
    public class AccountSummaryViewProjection : ViewProjection<AccountSummaryView, Guid>
    {
        public AccountSummaryViewProjection()
        {
            ProjectEvent<NewAccountCreated>((ev) => ev.AccountId, Persist);
            ProjectEvent<NewInflowRecorded>((ev) => ev.ToAccountId, Persist);
            ProjectEvent<NewOutflowRecorded>((ev) => ev.FromAccountId, Persist);
        }
        private void Persist(AccountSummaryView view, NewAccountCreated @event)
        {
            view.ApplyEvent(@event);
        }

        private void Persist(AccountSummaryView view, NewInflowRecorded @event)
        {
            view.ApplyEvent(@event);
        }

        private void Persist(AccountSummaryView view, NewOutflowRecorded @event)
        {
            view.ApplyEvent(@event);
        }
    }
}
