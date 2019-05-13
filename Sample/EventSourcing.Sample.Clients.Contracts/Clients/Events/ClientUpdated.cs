using System;
using Domain.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Events
{
    public class ClientUpdated: IEvent
    {
        public Guid ClientId { get; }
        public ClientInfo Data { get; }

        public ClientUpdated(Guid clientId, ClientInfo data)
        {
            ClientId = clientId;
            Data = data;
        }
    }
}
