using System;
using Core.Aggregates;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;

namespace EventSourcing.Sample.Clients.Domain.Clients
{
    public class Client: Aggregate
    {
        public string Name { get; private set; } = default!;

        public string Email { get; private set; } = default!;

        public Client()
        {
        }

        public Client(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;

            Enqueue(new ClientCreated(id, new ClientInfo(email,name)));
        }

        public void Update(ClientInfo clientInfo)
        {
            Name = clientInfo.Name;
            Email = clientInfo.Email;

            Enqueue(new ClientUpdated(Id, clientInfo));
        }
    }
}
