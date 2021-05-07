using System;
using Core.Commands;

namespace EventSourcing.Sample.Transactions.Contracts.Transactions.Commands
{
    public record MakeTransfer(
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount
    ): ICommand;
}
