using System;
using Core.Queries;
using EventSourcing.Sample.Transactions.Views.Clients;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Queries
{
    public class GetClientView: IQuery<ClientView>
    {
        public Guid Id { get; }

        public GetClientView(Guid id)
        {
            Id = id;
        }
    }
}
