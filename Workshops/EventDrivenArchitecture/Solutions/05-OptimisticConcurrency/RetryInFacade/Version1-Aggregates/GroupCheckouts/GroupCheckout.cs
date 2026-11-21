using System.Text.Json.Serialization;
using OptimisticConcurrency.RetryInFacade.Version1_Aggregates.Core;

namespace OptimisticConcurrency.RetryInFacade.Version1_Aggregates.GroupCheckouts;

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

public class GroupCheckout: Aggregate<GroupCheckoutEvent, Guid>
{
    [JsonInclude] private readonly Dictionary<Guid, CheckoutStatus> guestStayCheckouts = new();
    [JsonInclude] private CheckoutStatus status;

    [JsonConstructor]
    private GroupCheckout(
        Guid id,
        Dictionary<Guid, CheckoutStatus> guestStayCheckouts,
        CheckoutStatus status
    )
    {
        Id = id;
        this.guestStayCheckouts = guestStayCheckouts;
        this.status = status;
    }

    public static GroupCheckout Initiate(
        Guid groupCheckoutId,
        Guid clerkId,
        Guid[] guestStayAccountIds,
        DateTimeOffset initiatedAt
    )
    {
        var checkOut = new GroupCheckout(
            groupCheckoutId,
            guestStayAccountIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated),
            CheckoutStatus.Initiated
        );
        checkOut.Enqueue(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayAccountIds, initiatedAt));

        return checkOut;
    }


    public void RecordGuestCheckoutCompletion(
        Guid guestStayAccountId,
        DateTimeOffset now
    )
    {
        if (status != CheckoutStatus.Initiated || this.guestStayCheckouts[guestStayAccountId] == CheckoutStatus.Completed)
            return;

        var guestCheckoutCompleted = new GuestCheckoutCompletionRecorded(Id, guestStayAccountId, now);

        Enqueue(guestCheckoutCompleted);

        guestStayCheckouts[guestStayAccountId] = CheckoutStatus.Completed;

        if (!AreAnyOngoingCheckouts())
            Enqueue(Finish(now));
    }

    public void RecordGuestCheckoutFailure(
        Guid guestStayAccountId,
        DateTimeOffset now
    )
    {
        if (status != CheckoutStatus.Initiated || this.guestStayCheckouts[guestStayAccountId] == CheckoutStatus.Failed)
            return;

        var guestCheckoutFailed = new GuestCheckoutFailureRecorded(Id, guestStayAccountId, now);

        Enqueue(guestCheckoutFailed);

        guestStayCheckouts[guestStayAccountId] = CheckoutStatus.Failed;

        if (!AreAnyOngoingCheckouts())
            Enqueue(Finish(now));
    }

    private GroupCheckoutEvent Finish(DateTimeOffset now)
    {
        if (AreAnyFailedCheckouts())
        {
            status = CheckoutStatus.Failed;

            return new GroupCheckoutFailed
            (
                Id,
                CheckoutsWith(CheckoutStatus.Completed),
                CheckoutsWith(CheckoutStatus.Failed),
                now
            );
        }

        status = CheckoutStatus.Completed;

        return new GroupCheckoutCompleted(
            Id,
            CheckoutsWith(CheckoutStatus.Completed),
            now
        );
    }

    private bool AreAnyOngoingCheckouts() =>
        guestStayCheckouts.Values.Any(guestStayStatus => guestStayStatus is CheckoutStatus.Initiated);

    private bool AreAnyFailedCheckouts() =>
        guestStayCheckouts.Values.Any(guestStayStatus => guestStayStatus is CheckoutStatus.Failed);

    private Guid[] CheckoutsWith(CheckoutStatus guestStayStatus) =>
        guestStayCheckouts
            .Where(pair => pair.Value == guestStayStatus)
            .Select(pair => pair.Key)
            .ToArray();
}

public enum CheckoutStatus
{
    Initiated,
    Completed,
    Failed
}
