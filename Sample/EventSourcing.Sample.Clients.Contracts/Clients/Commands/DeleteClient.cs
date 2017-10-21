using Domain.Commands;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public class DeleteClient : ICommand
    {
        public Guid Id { get; }

        public DeleteClient(Guid id)
        {
            Id = id;
        }
    }
}
