using System.Text.Json.Serialization;
using BusinessProcesses.Version1_Aggregates.Core;

namespace BusinessProcesses.Version1_Aggregates.GuestStayAccounts;

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

    public static GuestStayAccount CheckIn(Guid guestStayAccountId, DateTimeOffset now)
    {
        var guestStay = new GuestStayAccount(guestStayAccountId, 0, GuestStayAccountStatus.Open);

        guestStay.Enqueue(new GuestCheckedIn(guestStayAccountId, now));

        return guestStay;
    }

    public void RecordCharge(decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        balance -= amount;

        Enqueue(new ChargeRecorded(Id, amount, now));
    }

    public void RecordPayment(decimal amount, DateTimeOffset now)
    {
        if (status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record payment for not opened account");

        balance += amount;

        Enqueue(new PaymentRecorded(Id, amount, now));
    }

    public void Checkout(DateTimeOffset now, Guid? groupCheckoutId = null)
    {
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
