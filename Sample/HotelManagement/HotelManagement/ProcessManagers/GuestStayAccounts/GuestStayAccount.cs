using Core.Aggregates;
using Core.Structures;
using Marten.Metadata;
using static Core.Structures.Result;

namespace HotelManagement.ProcessManagers.GuestStayAccounts;

public record GuestCheckedIn(
    Guid GuestStayId,
    DateTimeOffset CheckedInAt
);

public record GuestCheckedOut(
    Guid GuestStayId,
    DateTimeOffset CheckedOutAt,
    Guid? GroupCheckOutId = null
);

public record GuestCheckoutFailed(
    Guid GuestStayId,
    GuestCheckoutFailed.FailureReason Reason,
    DateTimeOffset FailedAt,
    Guid? GroupCheckOutId = null
)
{
    public enum FailureReason
    {
        NotOpened,
        BalanceNotSettled
    }
}

/// <summary>
/// This is an example of event-driven but not event-sourced aggregate
/// </summary>
public class GuestStayAccount: Aggregate, IVersioned
{
    private DateTimeOffset checkedInDate;
    private decimal balance;
    private GuestStayAccountStatus status;
    private DateTimeOffset? checkedOutDate;
    private bool IsSettled => balance == 0;

    // For Marten Optimistic Concurrency
    public new Guid Version { get; set; }

    private GuestStayAccount(
        Guid id,
        DateTimeOffset checkedInDate,
        decimal balance = 0,
        GuestStayAccountStatus status = GuestStayAccountStatus.Opened
    )
    {
        Id = id;
        this.checkedInDate = checkedInDate;
        this.balance = balance;
        this.status = status;
    }

    public static GuestStayAccount CheckIn(Guid guestStayId, DateTimeOffset now) => new(guestStayId, now);

    public void RecordCharge(decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        balance -= amount;
    }

    public void RecordPayment(decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        balance += amount;
    }

    public void CheckOut(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (status != GuestStayAccountStatus.Opened || !IsSettled)
        {
            Enqueue(new GuestCheckoutFailed(
                Id,
                status != GuestStayAccountStatus.Opened
                    ? GuestCheckoutFailed.FailureReason.NotOpened
                    : GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                now,
                groupCheckoutId
            ));
            return;
        }

        status = GuestStayAccountStatus.CheckedOut;
        checkedOutDate = now;

        Enqueue(
            new GuestCheckedOut(
                Id,
                now,
                groupCheckoutId
            )
        );
    }
}

public enum GuestStayAccountStatus
{
    Opened = 1,
    CheckedOut = 2
}
