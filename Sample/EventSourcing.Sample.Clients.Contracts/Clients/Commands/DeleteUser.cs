using Domain.Commands;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public class DeleteUser : ICommand
    {
        public Guid Id { get; }

        public DeleteUser(Guid id)
        {
            Id = id;
        }
    }
}
