using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using Marten.Events.Projections;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AccountSummaryViewProjection : ViewProjection<AccountsSummaryView>
    {
        public AccountSummaryViewProjection()
        {
            ProjectEvent<NewAccountCreated>(Persist);
        }

        private void Persist(AccountsSummaryView view, NewAccountCreated @event)
        {
            view.Apply(@event);
        }
    }
}
