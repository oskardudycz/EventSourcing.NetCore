using System;
using Core.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Events
{
    public record ClientUpdated(
        Guid ClientId,
        ClientInfo Data
    ): IEvent;
}
