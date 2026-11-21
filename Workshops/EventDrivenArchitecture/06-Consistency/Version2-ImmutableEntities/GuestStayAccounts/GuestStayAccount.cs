namespace Consistency.Version2_ImmutableEntities.GuestStayAccounts;

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

    public static GuestCheckedIn CheckIn(Guid guestStayAccountId, DateTimeOffset now) => new(guestStayAccountId, now);

    public ChargeRecorded RecordCharge(decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new ChargeRecorded(Id, amount, now);
    }

    public PaymentRecorded RecordPayment(decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record payment for not opened account");

        return new PaymentRecorded(Id, amount, now);
    }

    public GuestStayAccountEvent Checkout(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (Status != GuestStayAccountStatus.Open)
            return new GuestCheckoutFailed(
                Id,
                GuestCheckoutFailed.FailureReason.InvalidState,
                now,
                groupCheckoutId
            );

        return IsSettled
            ? new GuestCheckedOut(
                Id,
                now,
                groupCheckoutId
            )
            : new GuestCheckoutFailed(
                Id,
                GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                now,
                groupCheckoutId
            );
    }

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

    public static readonly GuestStayAccount Initial = new(default, default, default);
}

public enum GuestStayAccountStatus
{
    Open = 1,
    CheckedOut = 2
}
