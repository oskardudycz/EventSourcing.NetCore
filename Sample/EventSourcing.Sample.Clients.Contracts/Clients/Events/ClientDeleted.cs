using Domain.Events;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Events
{
    public class ClientDeleted : IEvent
    {
        public Guid Id { get; }

        public ClientDeleted(Guid id)
        {
            Id = id;
        }
    }
}
