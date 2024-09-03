namespace EntitiesDefinition.Solution2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountEvent;

public abstract record GuestStayAccountEvent
{
    public record GuestCheckedIn(
        Guid GuestStayId,
        DateTimeOffset CheckedInAt
    ): GuestStayAccountEvent;

    public record ChargeRecorded(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record PaymentRecorded(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record GuestCheckedOut(
        Guid GuestStayId,
        DateTimeOffset CheckedOutAt,
        Guid? GroupCheckOutId = null
    ): GuestStayAccountEvent;

    public record GuestCheckOutFailed(
        Guid GuestStayId,
        GuestCheckOutFailed.FailureReason Reason,
        DateTimeOffset FailedAt,
        Guid? GroupCheckOutId = null
    ): GuestStayAccountEvent
    {
        public enum FailureReason
        {
            NotOpened,
            BalanceNotSettled
        }
    }

    private GuestStayAccountEvent() { }
}

public record GuestStayAccount(
    Guid Id,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Opened
)
{
    public bool IsSettled => Balance == 0;

    public static GuestCheckedIn CheckIn(Guid guestStayId, DateTimeOffset now) => new(guestStayId, now);

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

    public GuestStayAccountEvent CheckOut(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (Status != GuestStayAccountStatus.Opened)
            return new GuestCheckOutFailed(
                Id,
                GuestCheckOutFailed.FailureReason.NotOpened,
                now,
                groupCheckoutId
            );

        return IsSettled
            ? new GuestCheckedOut(
                Id,
                now,
                groupCheckoutId
            )
            : new GuestCheckOutFailed(
                Id,
                GuestCheckOutFailed.FailureReason.BalanceNotSettled,
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
                Id = checkedIn.GuestStayId, Status = GuestStayAccountStatus.Opened
            },
            ChargeRecorded charge => this with { Balance = Balance - charge.Amount },
            PaymentRecorded payment => this with { Balance = Balance + payment.Amount },
            GuestCheckedOut => this with { Status = GuestStayAccountStatus.CheckedOut },
            GuestCheckOutFailed => this,
            _ => this
        };

    public static readonly GuestStayAccount Initial = new(default, default, default);
}

public enum GuestStayAccountStatus
{
    Opened = 1,
    CheckedOut = 2
}
