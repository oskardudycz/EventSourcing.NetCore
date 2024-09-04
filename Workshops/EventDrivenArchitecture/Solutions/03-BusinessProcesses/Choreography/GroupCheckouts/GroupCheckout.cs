using BusinessProcesses.Sagas.Version2_ImmutableEntities.Core;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GroupCheckoutEvent;

public abstract record GroupCheckoutEvent
{
    public record GroupCheckoutInitiated(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset InitiatedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutCompleted(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GuestCheckOutFailed(
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

public record GroupCheckOut(
    Guid Id,
    Dictionary<Guid, CheckoutStatus> GuestStayCheckouts,
    CheckoutStatus Status = CheckoutStatus.Initiated
)
{
    public static GroupCheckoutInitiated Initiate(Guid groupCheckoutId, Guid clerkId, Guid[] guestStayIds,
        DateTimeOffset initiatedAt) =>
        new(groupCheckoutId, clerkId, guestStayIds, initiatedAt);

    public GroupCheckoutEvent[] RecordGuestCheckoutCompletion(
        Guid guestStayId,
        DateTimeOffset now
    )
    {
        if (Status != CheckoutStatus.Initiated || GuestStayCheckouts[guestStayId] == CheckoutStatus.Completed)
            return [];

        var guestCheckoutCompleted = new GuestCheckoutCompleted(Id, guestStayId, now);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Completed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [guestCheckoutCompleted]
            : [guestCheckoutCompleted, Finalize(guestStayCheckouts, now)];
    }

    public GroupCheckoutEvent[] RecordGuestCheckoutFailure(
        Guid guestStayId,
        DateTimeOffset now
    )
    {
        if (Status != CheckoutStatus.Initiated || GuestStayCheckouts[guestStayId] == CheckoutStatus.Failed)
            return [];

        var guestCheckoutFailed = new GuestCheckOutFailed(Id, guestStayId, now);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Failed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [guestCheckoutFailed]
            : [guestCheckoutFailed, Finalize(guestStayCheckouts, now)];
    }

    private GroupCheckoutEvent Finalize(
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


    public GroupCheckOut Evolve(GroupCheckoutEvent @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated => this with
            {
                Id = initiated.GroupCheckoutId,
                GuestStayCheckouts = initiated.GuestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated),
                Status = CheckoutStatus.Initiated
            },
            GuestCheckoutCompleted guestCheckedOut => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOut.GuestStayId ? CheckoutStatus.Completed : vs.Value
                )
            },
            GuestCheckOutFailed guestCheckedOutFailed => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOutFailed.GuestStayId ? CheckoutStatus.Failed : vs.Value
                )
            },
            GroupCheckoutCompleted => this with { Status = CheckoutStatus.Completed },
            GroupCheckoutFailed => this with { Status = CheckoutStatus.Failed },
            _ => this
        };

    public static GroupCheckOut Initial = new(default, [], default);
}

public enum CheckoutStatus
{
    Initiated,
    Completed,
    Failed
}
