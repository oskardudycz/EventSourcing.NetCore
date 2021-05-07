using System;
using Core.Events;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.Events
{
    public record NewAccountCreated(
        Guid AccountId,
        Guid ClientId,
        string Number
    ): IEvent;
}
