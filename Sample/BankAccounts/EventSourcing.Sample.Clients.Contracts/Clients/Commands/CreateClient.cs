using System;
using Core.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public class CreateClient: ICommand
    {
        public Guid? Id { get; set; }
        public ClientInfo Data { get; }

        public CreateClient(Guid? id, ClientInfo data)
        {
            Id = id;
            Data = data;
        }
    }
}
