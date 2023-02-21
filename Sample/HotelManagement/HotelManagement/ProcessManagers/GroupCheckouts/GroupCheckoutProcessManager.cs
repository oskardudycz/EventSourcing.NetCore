using Core.ProcessManagers;
using HotelManagement.ProcessManagers.GuestStayAccounts;
using Marten.Metadata;

namespace HotelManagement.ProcessManagers.GroupCheckouts;

public record GroupCheckoutInitiated(
    Guid GroupCheckOutId,
    Guid ClerkId,
    Guid[] GuestStayIds,
    DateTimeOffset InitiatedAt
);
public record GroupCheckoutCompleted(
    Guid GroupCheckoutId,
    Guid[] CompletedCheckouts,
    DateTimeOffset CompletedAt
);

public record GroupCheckoutFailed(
    Guid GroupCheckoutId,
    Guid[] CompletedCheckouts,
    Guid[] FailedCheckouts,
    DateTimeOffset FailedAt
);

public enum CheckoutStatus
{
    Pending,
    Initiated,
    Completed,
    Failed
}

/// <summary>
/// This is an example of event-driven but not event-sourced process manager
/// </summary>
public class GroupCheckoutProcessManager: ProcessManager, IVersioned
{
    private Guid clerkId;
    private readonly Dictionary<Guid, CheckoutStatus> guestStayCheckouts;
    private CheckoutStatus status;
    private DateTimeOffset initiatedAt;
    private DateTimeOffset? completedAt;
    private DateTimeOffset? failedAt;

    // For Marten Optimistic Concurrency
    public new Guid Version { get; set; }

    private GroupCheckoutProcessManager(
        Guid id,
        Guid clerkId,
        Dictionary<Guid, CheckoutStatus> guestStayCheckouts,
        CheckoutStatus status,
        DateTimeOffset initiatedAt,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? failedAt = null
    )
    {
        Id = id;
        this.clerkId = clerkId;
        this.guestStayCheckouts = guestStayCheckouts;
        this.status = status;
        this.initiatedAt = initiatedAt;
        this.completedAt = completedAt;
        this.failedAt = failedAt;
    }

    public static GroupCheckoutProcessManager Initiate(
        Guid groupCheckoutId,
        Guid clerkId,
        Guid[] guestStayIds,
        DateTimeOffset initiatedAt
    )
    {
        var processManager = new GroupCheckoutProcessManager(
            groupCheckoutId,
            clerkId,
            guestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Pending),
            CheckoutStatus.Initiated,
            initiatedAt
        );

        processManager.EnqueueEvent(
            new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayIds, initiatedAt));

        return processManager;
    }


    public void Handle(GroupCheckoutInitiated @event)
    {
        if (status != CheckoutStatus.Initiated)
            return;

        foreach (var guestStayAccountId in @event.GuestStayIds)
        {
            if (guestStayCheckouts[guestStayAccountId] != CheckoutStatus.Pending) continue;

            guestStayCheckouts[guestStayAccountId] = CheckoutStatus.Initiated;

            ScheduleCommand(new CheckOutGuest(guestStayAccountId, @event.GroupCheckOutId));
        }
    }

    public void Handle(GuestCheckedOut @event)
    {
        if (guestStayCheckouts[@event.GuestStayId] == CheckoutStatus.Completed)
            return;

        guestStayCheckouts[@event.GuestStayId] = CheckoutStatus.Completed;

        TryFinishCheckout(@event.CheckedOutAt);
    }

    public void Handle(GuestCheckoutFailed @event)
    {
        if (guestStayCheckouts[@event.GuestStayId] == CheckoutStatus.Failed)
            return;

        guestStayCheckouts[@event.GuestStayId] = CheckoutStatus.Failed;

        TryFinishCheckout(@event.FailedAt);
    }

    private void TryFinishCheckout(DateTimeOffset now)
    {
        if (AreAnyOngoingCheckouts())
            return;

        if (AreAnyFailedCheckouts())
        {
            status = CheckoutStatus.Failed;
            failedAt = now;

            EnqueueEvent(
                new GroupCheckoutFailed(
                    Id,
                    CheckoutsWith(CheckoutStatus.Completed),
                    CheckoutsWith(CheckoutStatus.Failed),
                    now
                )
            );
        }
        else
        {
            status = CheckoutStatus.Completed;
            completedAt = now;

            EnqueueEvent(
                new GroupCheckoutCompleted(
                    Id,
                    CheckoutsWith(CheckoutStatus.Completed),
                    now
                )
            );
        }
    }

    private bool AreAnyOngoingCheckouts() =>
        guestStayCheckouts.Values.Any(checkoutStatus =>
            checkoutStatus is CheckoutStatus.Initiated or CheckoutStatus.Pending);

    private bool AreAnyFailedCheckouts() =>
        guestStayCheckouts.Values.Any(checkoutStatus => checkoutStatus is CheckoutStatus.Failed);

    private Guid[] CheckoutsWith(CheckoutStatus checkoutStatus) =>
        guestStayCheckouts
            .Where(pair => pair.Value == checkoutStatus)
            .Select(pair => pair.Key)
            .ToArray();
}
