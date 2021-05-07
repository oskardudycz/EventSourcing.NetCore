using System;
using System.Collections.Generic;
using Core.Queries;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Queries
{
    public record ClientListItem (
        Guid Id,
        string Name
    );

    public record GetClients: IQuery<List<ClientListItem>>;
}
