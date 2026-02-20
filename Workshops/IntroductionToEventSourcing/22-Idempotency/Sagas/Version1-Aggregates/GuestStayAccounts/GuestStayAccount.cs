using System.Text.Json.Serialization;
using Idempotency.Sagas.Version1_Aggregates.Core;

namespace Idempotency.Sagas.Version1_Aggregates.GuestStayAccounts;

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

public class GuestStayAccount: Aggregate<GuestStayAccountEvent, Guid>
{
    [JsonInclude] private decimal balance;
    [JsonInclude] private GuestStayAccountStatus status;
    private bool IsSettled => balance == 0;

    [JsonConstructor]
    private GuestStayAccount(
        Guid id,
        decimal balance,
        GuestStayAccountStatus status
    )
    {
        Id = id;
        this.balance = balance;
        this.status = status;
    }

    public static GuestStayAccount CheckIn(Guid guestStayId, DateTimeOffset now)
    {
        var guestStay = new GuestStayAccount(guestStayId, 0, GuestStayAccountStatus.Opened);

        guestStay.Enqueue(new GuestCheckedIn(guestStayId, now));

        return guestStay;
    }

    public void RecordCharge(decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        balance -= amount;

        Enqueue(new ChargeRecorded(Id, amount, now));
    }

    public void RecordPayment(decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        balance += amount;

        Enqueue(new PaymentRecorded(Id, amount, now));
    }

    public void CheckOut(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
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
