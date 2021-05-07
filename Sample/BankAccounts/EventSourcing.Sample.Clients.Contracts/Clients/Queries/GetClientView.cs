using System;
using Core.Queries;
using EventSourcing.Sample.Transactions.Views.Clients;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Queries
{
    public record GetClientView(
        Guid Id
    ): IQuery<ClientView>;
}
