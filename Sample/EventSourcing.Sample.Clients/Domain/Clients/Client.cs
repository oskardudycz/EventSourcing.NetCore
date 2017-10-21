using Domain.Aggregates;
using System;

namespace EventSourcing.Sample.Clients.Domain.Clients
{
    public class Client : IAggregate
    {
        public Guid Id { get; private set;  }

        public string Name { get; private set; }

        public string Email { get; private set; }

        public Client()
        {

        }

        public Client(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public void Update(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
