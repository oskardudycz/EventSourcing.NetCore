using System;
using Core.Aggregates;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;

namespace EventSourcing.Sample.Clients.Domain.Clients
{
    public class Client: Aggregate
    {
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

            Enqueue(new ClientCreated(id, new ClientInfo
            {
                Email = email,
                Name = name
            }));
        }

        public void Update(ClientInfo clientInfo)
        {
            Name = clientInfo.Name;
            Email = clientInfo.Email;

            Enqueue(new ClientUpdated(Id, clientInfo));
        }
    }
}
