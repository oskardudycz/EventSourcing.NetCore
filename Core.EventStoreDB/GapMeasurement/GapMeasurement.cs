using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;

namespace Core.EventStoreDB.GapMeasurement;

public static class GapMeasurement
{
    public static async Task<ulong> GetStreamSubscriptionGap
    (
        this EventStoreClient eventStore,
        string streamName,
        StreamPosition subscriptionLastPosition,
        CancellationToken cancellationToken
    )
    {
        var getCurrentLastEvent = await eventStore
            .ReadStreamAsync(
                Direction.Backwards,
                streamName,
                StreamPosition.End,
                1,
                resolveLinkTos: false,
                cancellationToken: cancellationToken
            ).LastAsync(cancellationToken);

        var currentLastPosition = getCurrentLastEvent.Event.EventNumber.ToUInt64();

        return subscriptionLastPosition - currentLastPosition;
    }

    public static async Task<ulong> GetAllStreamSubscriptionGap
    (
        this EventStoreClient eventStore,
        Position subscriptionLastPosition,
        CancellationToken cancellationToken
    )
    {
        var getCurrentLastEvent = await eventStore
            .ReadAllAsync(
                Direction.Backwards,
                Position.End,
                1,
                resolveLinkTos: false,
                cancellationToken: cancellationToken
            ).LastAsync(cancellationToken);

        var currentLastPosition = getCurrentLastEvent.Event.Position.CommitPosition;

        return subscriptionLastPosition.CommitPosition - currentLastPosition;
    }

    private const uint SubscriptionThreshold = 10;

    public static async Task<bool> IsLive(Func<Task<ulong>> getSubscriptionGap)
    {
        var subscriptionGap = await getSubscriptionGap();

        return subscriptionGap < SubscriptionThreshold;
    }
}
