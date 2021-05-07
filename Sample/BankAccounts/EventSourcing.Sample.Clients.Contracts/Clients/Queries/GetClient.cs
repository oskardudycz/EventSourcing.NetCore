using System;
using Core.Queries;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Queries
{
    public record ClientItem(
        Guid Id,
        string Name,
        string Email
    );

    public record GetClient(
        Guid Id
    ): IQuery<ClientItem>;
}
