using System;
using Core.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public class UpdateClient: ICommand
    {
        public Guid Id { get; }
        public ClientInfo Data { get; }

        public UpdateClient(Guid id, ClientInfo data)
        {
            Id = id;
            Data = data;
        }
    }
}
