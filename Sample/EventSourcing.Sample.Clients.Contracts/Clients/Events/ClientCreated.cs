using Domain.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Events
{
    public class ClientCreated : IEvent
    {
        public Guid Id { get; }
        public ClientInfo Data { get; }

        public ClientCreated(Guid id, ClientInfo data)
        {
            Id = id;
            Data = data;
        }
    }
}
