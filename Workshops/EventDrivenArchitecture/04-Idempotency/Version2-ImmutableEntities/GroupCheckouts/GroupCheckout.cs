using Idempotency.Version2_ImmutableEntities.Core;

namespace Idempotency.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutEvent;

public abstract record GroupCheckoutEvent
{
    public record GroupCheckoutInitiated(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayAccountIds,
        DateTimeOffset InitiatedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutCompletionRecorded(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutFailureRecorded(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset FailedAt
    ): GroupCheckoutEvent;

    public record GroupCheckoutCompleted(
        Guid GroupCheckoutId,
        Guid[] CompletedCheckouts,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GroupCheckoutFailed(
        Guid GroupCheckoutId,
        Guid[] CompletedCheckouts,
        Guid[] FailedCheckouts,
        DateTimeOffset FailedAt
    ): GroupCheckoutEvent;

    private GroupCheckoutEvent() { }
}

public record GroupCheckout(
    Guid Id,
    Dictionary<Guid, CheckoutStatus> GuestStayCheckouts,
    CheckoutStatus Status = CheckoutStatus.NotExisting
)
{
    public static GroupCheckoutInitiated Initiate(Guid groupCheckoutId, Guid clerkId, Guid[] guestStayAccountIds,
        DateTimeOffset initiatedAt) =>
        new(groupCheckoutId, clerkId, guestStayAccountIds, initiatedAt);

    public GroupCheckoutEvent[] RecordGuestCheckoutCompletion(
        Guid guestStayAccountId,
        DateTimeOffset now
    )
    {
        var guestCheckoutCompleted = new GuestCheckoutCompletionRecorded(Id, guestStayAccountId, now);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayAccountId, CheckoutStatus.Completed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [guestCheckoutCompleted]
            : [guestCheckoutCompleted, Finish(guestStayCheckouts, now)];
    }

    public GroupCheckoutEvent[] RecordGuestCheckoutFailure(
        Guid guestStayAccountId,
        DateTimeOffset now
    )
    {
        var guestCheckoutFailed = new GuestCheckoutFailureRecorded(Id, guestStayAccountId, now);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayAccountId, CheckoutStatus.Failed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [guestCheckoutFailed]
            : [guestCheckoutFailed, Finish(guestStayCheckouts, now)];
    }

    private GroupCheckoutEvent Finish(
        Dictionary<Guid, CheckoutStatus> guestStayCheckouts,
        DateTimeOffset now
    ) =>
        !AreAnyFailedCheckouts(guestStayCheckouts)
            ? new GroupCheckoutCompleted
            (
                Id,
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                now
            )
            : new GroupCheckoutFailed
            (
                Id,
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Failed),
                now
            );

    private static bool AreAnyOngoingCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
        guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Initiated);

    private static bool AreAnyFailedCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
        guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Failed);

    private static Guid[] CheckoutsWith(Dictionary<Guid, CheckoutStatus> guestStayCheckouts, CheckoutStatus status) =>
        guestStayCheckouts
            .Where(pair => pair.Value == status)
            .Select(pair => pair.Key)
            .ToArray();


    public GroupCheckout Evolve(GroupCheckoutEvent @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated => this with
            {
                Id = initiated.GroupCheckoutId,
                GuestStayCheckouts = initiated.GuestStayAccountIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated),
                Status = CheckoutStatus.Initiated
            },
            GuestCheckoutCompletionRecorded guestCheckedOut => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOut.GuestStayAccountId ? CheckoutStatus.Completed : vs.Value
                )
            },
            GuestCheckoutFailureRecorded guestCheckedOutFailed => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOutFailed.GuestStayAccountId ? CheckoutStatus.Failed : vs.Value
                )
            },
            GroupCheckoutCompleted => this with { Status = CheckoutStatus.Completed },
            GroupCheckoutFailed => this with { Status = CheckoutStatus.Failed },
            _ => this
        };

    public static GroupCheckout Initial = new(default, [], CheckoutStatus.NotExisting);
}

public enum CheckoutStatus
{
    NotExisting,
    Initiated,
    Completed,
    Failed
}
