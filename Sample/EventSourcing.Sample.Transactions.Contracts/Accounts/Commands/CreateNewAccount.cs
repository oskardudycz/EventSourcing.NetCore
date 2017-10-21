using Domain.Commands;
using System;

namespace EventSourcing.Sample.Tasks.Contracts.Accounts.Commands
{
    public class CreateNewAccount : ICommand
    {
        public Guid ClientId { get; set; }
    }
}
