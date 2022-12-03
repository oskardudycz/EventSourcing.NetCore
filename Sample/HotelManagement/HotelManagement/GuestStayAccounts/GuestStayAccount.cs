using Core.Structures;
using static Core.Structures.Result;

namespace HotelManagement.GuestStayAccounts;

public record GuestCheckedIn(
    Guid GuestStayId,
    DateTimeOffset CheckedInAt
);

public record ChargeRecorded(
    Guid GuestStayId,
    decimal Amount,
    DateTimeOffset RecordedAt
);

public record PaymentRecorded(
    Guid GuestStayId,
    decimal Amount,
    DateTimeOffset RecordedAt
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

public record GuestStayAccount(
    Guid Id,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Opened
)
{
    public bool IsSettled => Balance == 0;

    public static GuestCheckedIn CheckIn(Guid guestStayId, DateTimeOffset now) =>
        new GuestCheckedIn(guestStayId, now);

    public ChargeRecorded RecordCharge(decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new ChargeRecorded(Id, amount, now);
    }

    public PaymentRecorded RecordPayment(decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new PaymentRecorded(Id, amount, now);
    }

    public Result<GuestCheckedOut, GuestCheckoutFailed> CheckOut(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (Status != GuestStayAccountStatus.Opened)
            return Failure<GuestCheckedOut, GuestCheckoutFailed>(
                new GuestCheckoutFailed(
                    Id,
                    GuestCheckoutFailed.FailureReason.NotOpened,
                    now,
                    groupCheckoutId
                )
            );

        return IsSettled
            ? Success<GuestCheckedOut, GuestCheckoutFailed>(
                new GuestCheckedOut(
                    Id,
                    now,
                    groupCheckoutId
                )
            )
            : Failure<GuestCheckedOut, GuestCheckoutFailed>(
                new GuestCheckoutFailed(
                    Id,
                    GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                    now,
                    groupCheckoutId
                )
            );
    }

    public static GuestStayAccount Create(GuestCheckedIn @event) =>
        new GuestStayAccount(@event.GuestStayId);

    public GuestStayAccount Apply(ChargeRecorded @event) =>
        this with { Balance = Balance - @event.Amount };

    public GuestStayAccount Apply(PaymentRecorded @event) =>
        this with { Balance = Balance + @event.Amount };

    public GuestStayAccount Apply(GuestCheckedOut @event) =>
        this with { Status = GuestStayAccountStatus.CheckedOut };
}

public enum GuestStayAccountStatus
{
    Opened = 1,
    CheckedOut = 2
}
