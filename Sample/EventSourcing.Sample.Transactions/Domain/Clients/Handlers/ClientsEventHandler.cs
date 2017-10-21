using Domain.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using Marten;
using System.Threading.Tasks;

namespace EventSourcing.Sample.Transactions.Domain.Clients.Handlers
{
    public class ClientsEventHandler : 
        IAsyncEventHandler<ClientCreated>,
        IAsyncEventHandler<ClientUpdated>,
        IAsyncEventHandler<ClientDeleted>
    {
        private readonly IDocumentSession session;

        private Marten.Events.IEventStore store => session.Events;
        public ClientsEventHandler(IDocumentSession session)
        {
            this.session = session;
        }

        public Task Handle(ClientCreated @event)
        {
            store.Append(@event.Id, @event);
            return session.SaveChangesAsync();
        }

        public Task Handle(ClientUpdated @event)
        {
            store.Append(@event.Id, @event);
            return session.SaveChangesAsync();
        }

        public Task Handle(ClientDeleted @event)
        {
            store.Append(@event.Id, @event);
            return session.SaveChangesAsync();
        }
    }
}
