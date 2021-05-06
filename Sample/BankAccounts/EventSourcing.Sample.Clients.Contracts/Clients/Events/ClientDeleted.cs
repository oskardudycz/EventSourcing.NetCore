using System;
using Core.Events;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Events
{
    public record ClientDeleted(
        Guid ClientId
    ): IEvent;
}
