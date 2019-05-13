using System;
using System.Collections.Generic;
using Domain.Queries;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Queries
{
    public class ClientListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class GetClients: IQuery<List<ClientListItem>>
    {
    }
}
