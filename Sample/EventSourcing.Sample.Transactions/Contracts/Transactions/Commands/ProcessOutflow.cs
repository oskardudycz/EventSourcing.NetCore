using Domain.Commands;
using System;

namespace EventSourcing.Sample.Tasks.Contracts.Accounts.Commands
{
    public class ProcessOutflow : ICommand
    {
        public Guid FromAccountId { get; }
        public Guid ToAccountId { get; }
        public decimal Ammount { get; }

        public ProcessOutflow(decimal ammount, Guid fromAccountId, Guid toAccountId)
        {
            Ammount = ammount;
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
        }
    }
}
