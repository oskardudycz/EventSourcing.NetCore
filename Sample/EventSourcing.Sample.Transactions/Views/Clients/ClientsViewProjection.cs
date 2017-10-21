using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using Marten.Events.Projections;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Sample.Transactions.Views.Clients
{
    public class ClientsViewProjection : ViewProjection<ClientsView, Guid>
    {
        public ClientsViewProjection()
        {
            ProjectEvent<ClientCreated>(ev => ev.Id, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent<ClientUpdated>(ev => ev.Id, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent<ClientDeleted>(ev => ev.Id, (view, @event) => view.ApplyEvent(@event));
        }
    }
}
