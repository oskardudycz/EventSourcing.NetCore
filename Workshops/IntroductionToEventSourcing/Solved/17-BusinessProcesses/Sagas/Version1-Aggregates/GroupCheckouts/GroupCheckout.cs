using System.Text.Json.Serialization;
using BusinessProcesses.Sagas.Version1_Aggregates.Core;

namespace BusinessProcesses.Sagas.Version1_Aggregates.GroupCheckouts;

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

public class GroupCheckOut: Aggregate<GroupCheckoutEvent, Guid>
{
    [JsonInclude] private Dictionary<Guid, CheckoutStatus> guestStayCheckouts;
    [JsonInclude] private CheckoutStatus status;

    [JsonConstructor]
    private GroupCheckOut(
        Guid id,
        Dictionary<Guid, CheckoutStatus> guestStayCheckouts,
        CheckoutStatus status
    )
    {
        Id = id;
        this.guestStayCheckouts = guestStayCheckouts;
        this.status = status;
    }

    public static GroupCheckOut Initial() =>
        new GroupCheckOut(Guid.Empty, new Dictionary<Guid, CheckoutStatus>(), default);

    public static GroupCheckOut Initiate(
        Guid groupCheckoutId,
        Guid clerkId,
        Guid[] guestStayIds,
        DateTimeOffset initiatedAt
    )
    {
        var checkOut = new GroupCheckOut(
            groupCheckoutId,
            guestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated),
            CheckoutStatus.Initiated
        );
        checkOut.Enqueue(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayIds, initiatedAt));

        return checkOut;
    }


    public void RecordGuestCheckoutCompletion(
        Guid guestStayId,
        DateTimeOffset now
    )
    {
        if (status != CheckoutStatus.Initiated || this.guestStayCheckouts[guestStayId] == CheckoutStatus.Completed)
            return;

        var guestCheckoutCompleted = new GuestCheckoutCompleted(Id, guestStayId, now);

        Enqueue(guestCheckoutCompleted);

        guestStayCheckouts[guestStayId] = CheckoutStatus.Completed;

        if (!AreAnyOngoingCheckouts())
            Enqueue(Finalize(now));
    }

    public void RecordGuestCheckoutFailure(
        Guid guestStayId,
        DateTimeOffset now
    )
    {
        if (status != CheckoutStatus.Initiated || this.guestStayCheckouts[guestStayId] == CheckoutStatus.Failed)
            return;

        var guestCheckoutFailed = new GuestCheckOutFailed(Id, guestStayId, now);

        Enqueue(guestCheckoutFailed);

        if (!AreAnyOngoingCheckouts())
            Enqueue(Finalize(now));
    }

    private GroupCheckoutEvent Finalize(DateTimeOffset now) =>
        !AreAnyFailedCheckouts()
            ? new GroupCheckoutCompleted(
                Id,
                CheckoutsWith(CheckoutStatus.Completed),
                now
            )
            : new GroupCheckoutFailed
            (
                Id,
                CheckoutsWith(CheckoutStatus.Completed),
                CheckoutsWith(CheckoutStatus.Failed),
                now
            );

    private bool AreAnyOngoingCheckouts() =>
        guestStayCheckouts.Values.Any(guestStayStatus => guestStayStatus is CheckoutStatus.Initiated);

    private bool AreAnyFailedCheckouts() =>
        guestStayCheckouts.Values.Any(guestStayStatus => guestStayStatus is CheckoutStatus.Failed);

    private Guid[] CheckoutsWith(CheckoutStatus guestStayStatus) =>
        guestStayCheckouts
            .Where(pair => pair.Value == guestStayStatus)
            .Select(pair => pair.Key)
            .ToArray();


    public override void Apply(GroupCheckoutEvent @event)
    {
        switch (@event)
        {
            case GroupCheckoutInitiated initiated:
            {
                Id = initiated.GroupCheckoutId;
                guestStayCheckouts = initiated.GuestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated);
                status = CheckoutStatus.Initiated;
                break;
            }
            case GuestCheckoutCompleted guestCheckedOut:
            {
                guestStayCheckouts[guestCheckedOut.GuestStayId] = CheckoutStatus.Completed;
                break;
            }
            case GuestCheckOutFailed guestCheckedOutFailed:
            {
                guestStayCheckouts[guestCheckedOutFailed.GuestStayId] = CheckoutStatus.Failed;
                break;
            }
            case GroupCheckoutCompleted:
            {
                status = CheckoutStatus.Completed;
                break;
            }
            case GroupCheckoutFailed:
            {
                status = CheckoutStatus.Failed;
                break;
            }
        }
    }
}

public enum CheckoutStatus
{
    Initiated,
    Completed,
    Failed
}
