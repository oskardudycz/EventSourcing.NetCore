using System;
using Domain.Commands;

namespace EventSourcing.Sample.Tasks.Contracts.Accounts.Commands
{
    public class MakeTransfer: ICommand
    {
        public Guid FromAccountId { get; }
        public Guid ToAccountId { get; }
        public decimal Ammount { get; }

        public MakeTransfer(decimal ammount, Guid fromAccountId, Guid toAccountId)
        {
            Ammount = ammount;
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
        }
    }
}
