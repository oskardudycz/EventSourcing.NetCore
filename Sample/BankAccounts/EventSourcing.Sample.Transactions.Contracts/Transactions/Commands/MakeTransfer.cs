using System;
using Core.Commands;

namespace EventSourcing.Sample.Transactions.Contracts.Transactions.Commands
{
    public class MakeTransfer: ICommand
    {
        public Guid FromAccountId { get; }
        public Guid ToAccountId { get; }
        public decimal Amount { get; }

        public MakeTransfer(decimal amount, Guid fromAccountId, Guid toAccountId)
        {
            Amount = amount;
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
        }
    }
}
