using System;
using Core.Commands;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.Commands
{
    public class CreateNewAccount: ICommand
    {
        public Guid ClientId { get; set; }
    }
}
