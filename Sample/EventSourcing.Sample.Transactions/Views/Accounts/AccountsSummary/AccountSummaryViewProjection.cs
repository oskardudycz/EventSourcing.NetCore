using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using Marten.Events.Projections;
using System;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AccountSummaryViewProjection : ViewProjection<AccountsSummaryView>
    {
        readonly Guid viewid = new Guid("a8c1a4ac-686d-4fb7-a64a-710bc630f471");
        public AccountSummaryViewProjection()
        {
            ProjectEvent<NewAccountCreated>((ev)=> viewid, Persist);
        }

        private void Persist(AccountsSummaryView view, NewAccountCreated @event)
        {
            view.Apply(@event);
        }
    }
}
