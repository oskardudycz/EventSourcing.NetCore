using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Events;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Events;
using Marten;
using Marten.Events.Projections;

namespace EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary
{
    public class AccountSummaryViewProjection: ViewProjection<AccountSummaryView, Guid>
    {
        public AccountSummaryViewProjection()
        {
            Identity<NewAccountCreated>(ev => ev.AccountId);
            Identity<NewInflowRecorded>(ev => ev.ToAccountId);
            Identity<NewOutflowRecorded>(ev => ev.FromAccountId);

            ProjectEvent<NewAccountCreated>((view, @event) => view.Apply(@event));
            ProjectEvent<NewInflowRecorded>((view, @event) => view.Apply(@event));
            ProjectEvent<NewOutflowRecorded>((view, @event) => view.Apply(@event));
        }
    }
}
