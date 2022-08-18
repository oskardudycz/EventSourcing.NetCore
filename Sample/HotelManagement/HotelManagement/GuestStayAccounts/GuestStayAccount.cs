namespace HotelManagement.GuestStayAccounts;

public record GuestCheckedIn(
    Guid GuestStayId,
    DateTimeOffset CheckedInAt
);

public record ChargeRecorded(
    Guid GuestStayId,
    Decimal Amount,
    DateTimeOffset RecordedAt
);

public record PaymentRecorded(
    Guid GuestStayId,
    Decimal Amount,
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
        BalanceNotSettled
    }
}

public record GuestStayAccount(
    Guid Id,
    DateTimeOffset CheckedInAt,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Opened,
    DateTimeOffset? CheckedOutAt = null
)
{
    public bool IsSettled => Balance == 0;

    public static GuestStayAccount Create(GuestCheckedIn @event) =>
        new GuestStayAccount(@event.GuestStayId, @event.CheckedInAt);

    public GuestStayAccount Apply(ChargeRecorded @event) =>
        this with { Balance = Balance - @event.Amount };

    public GuestStayAccount Apply(PaymentRecorded @event) =>
        this with { Balance = Balance + @event.Amount };

    public GuestStayAccount Apply(GuestCheckedOut @event) =>
        this with { Status = GuestStayAccountStatus.CheckedOut };
}

public enum GuestStayAccountStatus
{
    Opened,
    CheckedOut
}
