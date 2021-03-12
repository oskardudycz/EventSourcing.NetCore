using System;
using System.Collections.Generic;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace EventSourcing.Sample.Transactions.Views.Clients
{
    public class ClientsViewProjection: AggregateProjection<ClientView>
    {
        public ClientsViewProjection()
        {
            DeleteEvent<ClientDeleted>();
        }

        public ClientView Create(ClientCreated @event)
        {
            return new ClientView
            {
                Id = @event.ClientId,
                Name = @event.Data.Name,
                Email = @event.Data.Email,
                AccountsNumbers = new List<string>()
            };
        }

        public void Apply(ClientUpdated @event, ClientView view)
        {
            view.Name = @event.Data.Name;
            view.Email = @event.Data.Email;
        }

        public void Apply(NewAccountCreated @event, ClientView view)
        {
            view.AccountsNumbers.Add(@event.Number);
        }
    }
}
