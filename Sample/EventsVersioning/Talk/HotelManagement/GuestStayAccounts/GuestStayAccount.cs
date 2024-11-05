namespace HotelManagement.GuestStayAccounts;

using static GuestStayAccountEvent;

public record GuestStayAccount(
    string Id,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Opened
)
{
    public bool IsSettled => Balance == 0;

    public GuestStayAccount Evolve(GuestStayAccountEvent @event) =>
        @event switch
        {
            GuestCheckedIn checkedIn => this with
            {
                Id = checkedIn.GuestStayAccountId, Status = GuestStayAccountStatus.Opened
            },
            ChargeRecorded charge => this with { Balance = Balance - charge.Amount },
            PaymentRecorded payment => this with { Balance = Balance + payment.Amount },
            GuestCheckedOut => this with { Status = GuestStayAccountStatus.CheckedOut },
            GuestCheckoutFailed => this,
            _ => this
        };

    public static readonly GuestStayAccount Initial = new("", default, default);
}

public enum GuestStayAccountStatus
{
    Opened = 1,
    CheckedOut = 2
}

public abstract record GuestStayAccountEvent
{
    public record GuestCheckedIn(
        string GuestStayAccountId,
        string GuestStayId,
        string RoomId,
        string ClerkId,
        DateTimeOffset CheckedInAt
    ): GuestStayAccountEvent;

    public record ChargeRecorded(
        string GuestStayAccountId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record PaymentRecorded(
        string GuestStayAccountId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record GuestCheckedOut(
        string GuestStayAccountId,
        string ClerkId,
        DateTimeOffset CheckedOutAt
    ): GuestStayAccountEvent;

    public record GuestCheckoutFailed(
        string GuestStayAccountId,
        string ClerkId,
        GuestCheckoutFailed.FailureReason Reason,
        DateTimeOffset FailedAt
    ): GuestStayAccountEvent
    {
        public enum FailureReason
        {
            NotOpened,
            BalanceNotSettled
        }
    }

    private GuestStayAccountEvent(){}
}
