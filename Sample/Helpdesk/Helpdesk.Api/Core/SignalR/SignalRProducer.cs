using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using Microsoft.AspNetCore.SignalR;

namespace Helpdesk.Api.Core.SignalR;

public class SignalRProducer<THub>(IHubContext<THub> hubContext): SubscriptionBase where THub : Hub
{
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange eventRange,
        ISubscriptionController subscriptionController,
        IDocumentOperations operations,
        CancellationToken ct
    )
    {
        foreach (var @event in eventRange.Events)
        {
            try
            {
                await hubContext.Clients.All.SendAsync(@event.EventTypeName, @event.Data, ct);
            }
            catch (Exception exc)
            {
                // this is fine to put event to dead letter queue, as it's just SignalR notification
                await subscriptionController.RecordDeadLetterEventAsync(@event, exc);
            }
        }

        return NullChangeListener.Instance;
    }
}
