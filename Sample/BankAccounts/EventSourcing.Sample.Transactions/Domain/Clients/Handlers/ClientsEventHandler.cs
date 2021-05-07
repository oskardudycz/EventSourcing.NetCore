using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using Marten;

namespace EventSourcing.Sample.Transactions.Domain.Clients.Handlers
{
    public class ClientsEventHandler:
        IEventHandler<ClientCreated>,
        IEventHandler<ClientUpdated>,
        IEventHandler<ClientDeleted>
    {
        private readonly IDocumentSession session;

        private Marten.Events.IEventStore Store => session.Events;

        public ClientsEventHandler(IDocumentSession session)
        {
            this.session = session;
        }

        public Task Handle(ClientCreated @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Store.Append(@event.ClientId, @event);
            return session.SaveChangesAsync(cancellationToken);
        }

        public Task Handle(ClientUpdated @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Store.Append(@event.ClientId, @event);
            return session.SaveChangesAsync(cancellationToken);
        }

        public Task Handle(ClientDeleted @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            Store.Append(@event.ClientId, @event);
            return session.SaveChangesAsync(cancellationToken);
        }
    }
}
