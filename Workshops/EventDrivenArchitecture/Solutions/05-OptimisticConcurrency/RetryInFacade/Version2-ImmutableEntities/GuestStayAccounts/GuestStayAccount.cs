namespace OptimisticConcurrency.RetryInFacade.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountEvent;

public abstract record GuestStayAccountEvent
{
    public record GuestCheckedIn(
        Guid GuestStayAccountId,
        DateTimeOffset OpenedAt
    ): GuestStayAccountEvent;

    public record ChargeRecorded(
        Guid GuestStayAccountId,
        Guid ChargeId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record PaymentRecorded(
        Guid GuestStayAccountId,
        Guid PaymentId,
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
    Guid[] RecordedChargeIds,
    Guid[] RecordedPaymentIds,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Open,
    Guid? GroupCheckoutId = null
)
{
    public bool IsSettled => Balance == 0;

    public static GuestCheckedIn CheckIn(Guid guestStayAccountId, DateTimeOffset now) => new(guestStayAccountId, now);

    public ChargeRecorded[] RecordCharge(Guid chargeId, decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        if (RecordedChargeIds.Contains(chargeId))
            return [];

        return [new ChargeRecorded(Id, chargeId, amount, now)];
    }

    public PaymentRecorded[] RecordPayment(Guid paymentId, decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record payment for not opened account");

        if (RecordedPaymentIds.Contains(paymentId))
            return [];

        return [new PaymentRecorded(Id, paymentId, amount, now)];
    }

    public GuestStayAccountEvent[] Checkout(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (Status == GuestStayAccountStatus.CheckedOut && GroupCheckoutId == groupCheckoutId)
            return [];

        if (Status != GuestStayAccountStatus.Open)
            return
            [
                new GuestCheckoutFailed(
                    Id,
                    GuestCheckoutFailed.FailureReason.InvalidState,
                    now,
                    groupCheckoutId
                )
            ];

        return IsSettled
            ?
            [
                new GuestCheckedOut(
                    Id,
                    now,
                    groupCheckoutId
                )
            ]
            :
            [
                new GuestCheckoutFailed(
                    Id,
                    GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                    now,
                    groupCheckoutId
                )
            ];
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
            ChargeRecorded charge => this with
            {
                Balance = Balance - charge.Amount, RecordedChargeIds = [..RecordedChargeIds, charge.ChargeId]
            },
            PaymentRecorded payment => this with
            {
                Balance = Balance + payment.Amount, RecordedPaymentIds = [..RecordedPaymentIds, payment.PaymentId]
            },
            GuestCheckedOut checkedOut => this with
            {
                Status = GuestStayAccountStatus.CheckedOut, GroupCheckoutId = checkedOut.GroupCheckoutId
            },
            GuestCheckoutFailed => this,
            _ => this
        };

    public static readonly GuestStayAccount Initial = new(default, [], [], default, GuestStayAccountStatus.NotExisting);
}

public enum GuestStayAccountStatus
{
    NotExisting = 0,
    Open = 1,
    CheckedOut = 2
}
