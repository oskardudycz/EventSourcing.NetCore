using System;
using Domain.Commands;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public class DeleteClient: ICommand
    {
        public Guid Id { get; }

        public DeleteClient(Guid id)
        {
            Id = id;
        }
    }
}
