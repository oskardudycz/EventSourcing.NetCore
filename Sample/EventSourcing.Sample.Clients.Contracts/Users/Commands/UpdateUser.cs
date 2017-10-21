using Domain.Commands;
using System;

namespace EventSourcing.Sample.Clients.Contracts.Users.Commands
{
    public class UpdateUser : ICommand
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Email { get; }

        public UpdateUser(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
    }
}
