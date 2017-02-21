using Marten.Events.Projections;
using System;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Accounts;
using System.Collections.Generic;

namespace EventSourcing.Sample.Tasks.Views.Accounts
{
    public class AccountsSummaryView
    {
        public Guid Id { get; set; }
        public IList<AccountSummary> Accounts { get; } = new List<AccountSummary>();

        public void Apply(NewAccountCreated @event)
        {
            Accounts.Add(new AccountSummary()
            {
                ClientId = @event.ClientId
            });
        }
    }
}
