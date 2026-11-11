using System.Text.Json.Serialization;
using Idempotency.Core;

namespace Idempotency.Version1_Aggregates.GuestStayAccounts;

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

public class GuestStayAccount: Aggregate<GuestStayAccountEvent, Guid>
{
    [JsonInclude] private decimal balance;
    [JsonInclude] private GuestStayAccountStatus status;
    [JsonInclude] private List<Guid> recordedChargeIds;
    [JsonInclude] private List<Guid> recordedPaymentIds;
    [JsonInclude] private Guid? groupCheckOutId;
    private bool IsSettled => balance == 0;

    [JsonConstructor]
    private GuestStayAccount(
        Guid id,
        decimal balance,
        GuestStayAccountStatus status,
        List<Guid> recordedChargeIds,
        List<Guid> recordedPaymentIds,
        Guid? groupCheckOutId
    )
    {
        Id = id;
        this.balance = balance;
        this.status = status;
        this.recordedChargeIds = recordedChargeIds;
        this.recordedPaymentIds = recordedPaymentIds;
        this.groupCheckOutId = groupCheckOutId;
    }

    public static GuestStayAccount CheckIn(Guid guestStayId, DateTimeOffset now)
    {
        var guestStay = new GuestStayAccount(guestStayId, 0, GuestStayAccountStatus.Opened, [], [], null);

        guestStay.Enqueue(new GuestCheckedIn(guestStayId, now));

        return guestStay;
    }

    public void RecordCharge(Guid chargeId, decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        if (recordedChargeIds.Contains(chargeId))
            return;

        balance -= amount;
        recordedChargeIds.Add(chargeId);

        Enqueue(new ChargeRecorded(Id, chargeId, amount, now));
    }

    public void RecordPayment(Guid paymentId, decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        if (recordedPaymentIds.Contains(paymentId))
            return;

        balance += amount;
        recordedPaymentIds.Add(paymentId);

        Enqueue(new PaymentRecorded(Id, paymentId, amount, now));
    }

    public void CheckOut(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
        if (status == GuestStayAccountStatus.CheckedOut && groupCheckOutId == groupCheckoutId)
            return;

        if (status != GuestStayAccountStatus.Opened || !IsSettled)
        {
            Enqueue(new GuestCheckOutFailed(
                Id,
                status != GuestStayAccountStatus.Opened
                    ? GuestCheckOutFailed.FailureReason.NotOpened
                    : GuestCheckOutFailed.FailureReason.BalanceNotSettled,
                now,
                groupCheckoutId
            ));
            return;
        }

        status = GuestStayAccountStatus.CheckedOut;
        groupCheckOutId = groupCheckoutId;

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
