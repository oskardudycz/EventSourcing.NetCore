using Domain.Aggregates;
using System;

namespace EventSourcing.Sample.Clients.Domain.Users
{
    public class User : IAggregate
    {
        public Guid Id { get; set;  }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
