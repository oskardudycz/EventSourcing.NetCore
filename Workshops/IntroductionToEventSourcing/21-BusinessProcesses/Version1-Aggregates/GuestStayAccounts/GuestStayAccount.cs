using System.Text.Json.Serialization;
using BusinessProcesses.Version1_Aggregates.Core;

namespace BusinessProcesses.Version1_Aggregates.GuestStayAccounts;

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

    public static GuestStayAccount Initial() => new GuestStayAccount(Guid.Empty, 0, default);

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

        Enqueue(new ChargeRecorded(Id, amount, now));
    }

    public void RecordPayment(decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

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

        Enqueue(
            new GuestCheckedOut(
                Id,
                now,
                groupCheckoutId
            )
        );
    }

    public override void Apply(GuestStayAccountEvent @event)
    {
        switch (@event)
        {
            case GuestCheckedIn guestCheckedIn:
            {
                Id = guestCheckedIn.GuestStayId;
                balance = 0;
                status = GuestStayAccountStatus.Opened;
                return;
            }
            case ChargeRecorded chargeRecorded:
            {
                balance -= chargeRecorded.Amount;
                return;
            }
            case PaymentRecorded paymentRecorded:
            {
                balance += paymentRecorded.Amount;
                return;
            }
            case GuestCheckedOut:
            {
                status = GuestStayAccountStatus.CheckedOut;
                return;
            }
            default:
                return;
        }
    }
}

public enum GuestStayAccountStatus
{
    Opened = 1,
    CheckedOut = 2
}
