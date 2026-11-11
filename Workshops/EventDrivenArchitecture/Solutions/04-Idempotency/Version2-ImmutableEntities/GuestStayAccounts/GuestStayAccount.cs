namespace Idempotency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountEvent;

public abstract record GuestStayAccountEvent
{
    public record GuestCheckedIn(
        Guid GuestStayId,
        DateTimeOffset CheckedInAt
    ): GuestStayAccountEvent;

    public record ChargeRecorded(
        Guid GuestStayId,
        Guid ChargeId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record PaymentRecorded(
        Guid GuestStayId,
        Guid PaymentId,
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
    Guid[] RecordedChargeIds,
    Guid[] RecordedPaymentIds,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Opened,
    Guid? GroupCheckOutId = null
)
{
    public bool IsSettled => Balance == 0;

    public static GuestCheckedIn CheckIn(Guid guestStayId, DateTimeOffset now) => new(guestStayId, now);

    public ChargeRecorded[] RecordCharge(Guid chargeId, decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        if (RecordedChargeIds.Contains(chargeId))
            return [];

        return [new ChargeRecorded(Id, chargeId, amount, now)];
    }

    public PaymentRecorded[] RecordPayment(Guid paymentId, decimal amount, DateTimeOffset now)
    {
        if (Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        if (RecordedPaymentIds.Contains(paymentId))
            return [];

        return [new PaymentRecorded(Id, paymentId, amount, now)];
    }

    public GuestStayAccountEvent[] CheckOut(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (Status == GuestStayAccountStatus.CheckedOut && GroupCheckOutId == groupCheckoutId)
            return [];

        if (Status != GuestStayAccountStatus.Opened)
            return
            [
                new GuestCheckOutFailed(
                    Id,
                    GuestCheckOutFailed.FailureReason.NotOpened,
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
                new GuestCheckOutFailed(
                    Id,
                    GuestCheckOutFailed.FailureReason.BalanceNotSettled,
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
                Id = checkedIn.GuestStayId, Status = GuestStayAccountStatus.Opened
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
                Status = GuestStayAccountStatus.CheckedOut, GroupCheckOutId = checkedOut.GroupCheckOutId
            },
            GuestCheckOutFailed => this,
            _ => this
        };

    public static readonly GuestStayAccount Initial = new(default, [], [], default, default);
}

public enum GuestStayAccountStatus
{
    Opened = 1,
    CheckedOut = 2
}
