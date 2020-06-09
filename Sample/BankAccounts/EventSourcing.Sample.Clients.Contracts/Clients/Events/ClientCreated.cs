using System;
using Domain.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Events
{
    public class ClientCreated: IEvent
    {
        public Guid ClientId { get; }
        public ClientInfo Data { get; }

        public ClientCreated(Guid clientId, ClientInfo data)
        {
            ClientId = clientId;
            Data = data;
        }
    }
}
