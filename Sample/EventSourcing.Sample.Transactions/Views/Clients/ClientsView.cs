using Marten.Events.Projections;
using System;
using System.Collections.Generic;
using System.Text;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;

namespace EventSourcing.Sample.Transactions.Views.Clients
{
    public class ClientsView : ViewProjection<ClientsView, Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        internal void ApplyEvent(ClientCreated @event)
        {
            Id = @event.Id;
            Name = @event.Data.Name;
            IsDeleted = false;
        }

        internal void ApplyEvent(ClientUpdated @event)
        {
            Name = @event.Data.Name;
        }

        internal void ApplyEvent(ClientDeleted @event)
        {
            IsDeleted = true;
        }
    }
}
