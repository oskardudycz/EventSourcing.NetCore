namespace BusinessProcesses.Choreography.GuestStayAccounts;

using static GuestStayAccountEvent;

public abstract record GuestStayAccountEvent
{
    public record GuestCheckedIn(
        Guid GuestStayAccountId,
        DateTimeOffset OpenedAt
    ): GuestStayAccountEvent;

    public record ChargeRecorded(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record PaymentRecorded(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record GuestCheckedOut(
        Guid GuestStayAccountId,
        DateTimeOffset CompletedAt,
        Guid? GroupCheckoutId = null
    ): GuestStayAccountEvent;

    public record GuestCheckoutFailed(
        Guid GuestStayAccountId,
        GuestCheckoutFailed.FailureReason Reason,
        DateTimeOffset FailedAt,
        Guid? GroupCheckoutId = null
    ): GuestStayAccountEvent
    {
        public enum FailureReason
        {
            InvalidState,
            BalanceNotSettled
        }
    }

    private GuestStayAccountEvent() { }
}

public record GuestStayAccount(
    Guid Id,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Open
)
{
    public bool IsSettled => Balance == 0;

    // This method can be used to build state from events
    // You can ignore it if you're not into Event Sourcing
    public GuestStayAccount Evolve(GuestStayAccountEvent @event) =>
        @event switch
        {
            GuestCheckedIn checkedIn => this with
            {
                Id = checkedIn.GuestStayAccountId, Status = GuestStayAccountStatus.Open
            },
            ChargeRecorded charge => this with { Balance = Balance - charge.Amount },
            PaymentRecorded payment => this with { Balance = Balance + payment.Amount },
            GuestCheckedOut => this with { Status = GuestStayAccountStatus.CheckedOut },
            GuestCheckoutFailed => this,
            _ => this
        };

    public static readonly GuestStayAccount Initial = new(default, default, GuestStayAccountStatus.NotExisting);
}

public enum GuestStayAccountStatus
{
    NotExisting = 0,
    Open = 1,
    CheckedOut = 2
}
