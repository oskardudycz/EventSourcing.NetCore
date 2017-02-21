using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using Marten.Events.Projections;
using System;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AccountSummaryViewProjection : ViewProjection<AccountsSummaryView>
    {
        public AccountSummaryViewProjection()
        {
            ProjectEvent<NewAccountCreated>((ev)=>Guid.Empty,Persist);
        }

        private void Persist(AccountsSummaryView view, NewAccountCreated @event)
        {
            view.ApplyEvent(@event);
        }
    }
}
