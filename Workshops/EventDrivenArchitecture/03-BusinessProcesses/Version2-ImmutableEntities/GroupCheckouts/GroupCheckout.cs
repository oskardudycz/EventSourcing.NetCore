using BusinessProcesses.Version2_ImmutableEntities.Core;

namespace BusinessProcesses.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutEvent;

public abstract record GroupCheckoutEvent
{
    public record GroupCheckoutInitiated(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset InitiatedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutsInitiated(
        Guid GroupCheckoutId,
        Guid[] InitiatedGuestStayIds,
        DateTimeOffset InitiatedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutCompleted(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutFailed(
        Guid GroupCheckoutId,
        Guid GuestStayId,
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
    CheckoutStatus Status = CheckoutStatus.Initiated
)
{
    public static GroupCheckoutInitiated Initiate(Guid groupCheckoutId, Guid clerkId, Guid[] guestStayIds,
        DateTimeOffset initiatedAt) =>
        new(groupCheckoutId, clerkId, guestStayIds, initiatedAt);

    public GuestCheckoutsInitiated? RecordGuestCheckoutsInitiation(
        Guid[] initiatedGuestStayIds,
        DateTimeOffset now
    ) =>
        Status == CheckoutStatus.Initiated ? new GuestCheckoutsInitiated(Id, initiatedGuestStayIds, now) : null;

    public (GuestCheckoutCompleted?, (GroupCheckoutCompleted?, GroupCheckoutFailed?)?) RecordGuestCheckoutCompletion(
        Guid guestStayId,
        DateTimeOffset now
    )
    {
        if (Status == CheckoutStatus.Initiated && GuestStayCheckouts[guestStayId] != CheckoutStatus.Completed)
            return (null, null);

        var guestCheckoutCompleted = new GuestCheckoutCompleted(Id, guestStayId, now);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Completed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? (guestCheckoutCompleted, null)
            : (guestCheckoutCompleted, Finalize(guestStayCheckouts, now));
    }

    public (GuestCheckoutFailed?, (GroupCheckoutCompleted?, GroupCheckoutFailed?)?) RecordGuestCheckoutFailure(
        Guid guestStayId,
        DateTimeOffset now
    )
    {
        if (Status == CheckoutStatus.Initiated && GuestStayCheckouts[guestStayId] != CheckoutStatus.Completed)
            return (null, null);

        var guestCheckoutFailed = new GuestCheckoutFailed(Id, guestStayId, now);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Failed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? (guestCheckoutFailed, null)
            : (guestCheckoutFailed, Finalize(guestStayCheckouts, now));
    }

    private (GroupCheckoutCompleted?, GroupCheckoutFailed?) Finalize(
        Dictionary<Guid, CheckoutStatus> guestStayCheckouts,
        DateTimeOffset now
    ) =>
        !AreAnyFailedCheckouts(guestStayCheckouts)
            ? (new GroupCheckoutCompleted
            (
                Id,
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                now
            ), null)
            : (null, new GroupCheckoutFailed
            (
                Id,
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Failed),
                now
            ));

    private static bool AreAnyOngoingCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
        guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Initiated or CheckoutStatus.Pending);

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
                GuestStayCheckouts = initiated.GuestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Pending),
                Status = CheckoutStatus.Initiated
            },
            GuestCheckoutsInitiated checkoutsInitiated => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => checkoutsInitiated.InitiatedGuestStayIds.ToDictionary(
                        id => id,
                        _ => CheckoutStatus.Initiated
                    ).GetValueOrDefault(vs.Key, vs.Value)
                )
            },
            GuestCheckoutCompleted guestCheckedOut => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOut.GuestStayId ? CheckoutStatus.Completed : vs.Value
                )
            },
            GuestCheckoutFailed guestCheckedOutFailed => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOutFailed.GuestStayId ? CheckoutStatus.Completed : vs.Value
                )
            },
            GroupCheckoutCompleted => this with { Status = CheckoutStatus.Completed },
            GroupCheckoutFailed => this with { Status = CheckoutStatus.Failed },
            _ => this
        };

    public static GroupCheckout Initial = new(default, [], default);
}

public enum CheckoutStatus
{
    Pending,
    Initiated,
    Completed,
    Failed
}
