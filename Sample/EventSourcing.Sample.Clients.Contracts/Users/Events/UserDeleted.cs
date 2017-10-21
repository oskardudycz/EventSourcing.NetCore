using Domain.Events;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Users.Events
{
    public class UserDeleted : IEvent
    {
        public Guid Id { get; }

        public UserDeleted(Guid id)
        {
            Id = id;
        }
    }
}
