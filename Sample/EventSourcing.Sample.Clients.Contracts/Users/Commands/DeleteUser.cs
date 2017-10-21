using Domain.Commands;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Users.Commands
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
