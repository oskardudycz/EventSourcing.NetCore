using System;
using Domain.Events;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Events
{
    public class ClientDeleted: IEvent
    {
        public Guid ClientId { get; }

        public ClientDeleted(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}
