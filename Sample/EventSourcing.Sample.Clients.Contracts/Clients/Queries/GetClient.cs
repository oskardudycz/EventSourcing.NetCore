using Domain.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Queries
{
    public class ClientItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class GetClient : IQuery<ClientItem>
    {
        public Guid Id { get; }

        public GetClient(Guid id)
        {
            Id = id;
        }

    }
}
