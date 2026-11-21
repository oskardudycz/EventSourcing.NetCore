using System.Text.Json.Serialization;
using OptimisticConcurrency.RetryInFacade.Version1_Aggregates.Core;

namespace OptimisticConcurrency.RetryInFacade.Version1_Aggregates.GuestStayAccounts;

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

public class GuestStayAccount: Aggregate<GuestStayAccountEvent, Guid>
{
    [JsonInclude] private decimal balance;
    [JsonInclude] private GuestStayAccountStatus status;
    [JsonInclude] private List<Guid> recordedChargeIds;
    [JsonInclude] private List<Guid> recordedPaymentIds;
    [JsonInclude] private Guid? groupCheckoutId;
    private bool IsSettled => balance == 0;

    [JsonConstructor]
    private GuestStayAccount(
        Guid id,
        decimal balance,
        GuestStayAccountStatus status,
        List<Guid> recordedChargeIds,
        List<Guid> recordedPaymentIds,
        Guid? groupCheckoutId
    )
    {
        Id = id;
        this.balance = balance;
        this.status = status;
        this.recordedChargeIds = recordedChargeIds;
        this.recordedPaymentIds = recordedPaymentIds;
        this.groupCheckoutId = groupCheckoutId;
    }

    public static GuestStayAccount CheckIn(Guid guestStayAccountId, DateTimeOffset now)
    {
        var guestStay = new GuestStayAccount(guestStayAccountId, 0, GuestStayAccountStatus.Open, [], [], null);

        guestStay.Enqueue(new GuestCheckedIn(guestStayAccountId, now));

        return guestStay;
    }

    public void RecordCharge(Guid chargeId, decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        if (recordedChargeIds.Contains(chargeId))
            return;

        balance -= amount;
        recordedChargeIds.Add(chargeId);

        Enqueue(new ChargeRecorded(Id, chargeId, amount, now));
    }

    public void RecordPayment(Guid paymentId, decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record payment for not opened account");

        if (recordedPaymentIds.Contains(paymentId))
            return;

        balance += amount;
        recordedPaymentIds.Add(paymentId);

        Enqueue(new PaymentRecorded(Id, paymentId, amount, now));
    }

    public void Checkout(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (status == GuestStayAccountStatus.CheckedOut && this.groupCheckoutId == groupCheckoutId)
            return;

        if (status != GuestStayAccountStatus.Open || !IsSettled)
        {
            Enqueue(new GuestCheckoutFailed(
                Id,
                status != GuestStayAccountStatus.Open
                    ? GuestCheckoutFailed.FailureReason.InvalidState
                    : GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                now,
                groupCheckoutId
            ));
            return;
        }

        status = GuestStayAccountStatus.CheckedOut;
        this.groupCheckoutId = groupCheckoutId;

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
    Open = 1,
    CheckedOut = 2
}
