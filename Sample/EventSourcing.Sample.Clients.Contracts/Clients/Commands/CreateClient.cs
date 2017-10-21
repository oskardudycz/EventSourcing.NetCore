using Domain.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public class CreateClient : ICommand
    {
        public Guid? Id { get; }
        public ClientInfo Data { get; }

        public CreateClient(Guid id, ClientInfo data)
        {
            Id = id;
            Data = data;
        }
    }
}
