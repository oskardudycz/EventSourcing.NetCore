using Domain.Events;
using EventSourcing.Sample.Clients.Contracts.Users.DTOs;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Users.Events
{
    public class UserCreated : IEvent
    {
        public Guid Id { get; }
        public UserInfo Data { get; }

        public UserCreated(Guid id, UserInfo data)
        {
            Id = id;
            Data = data;
        }
    }
}
