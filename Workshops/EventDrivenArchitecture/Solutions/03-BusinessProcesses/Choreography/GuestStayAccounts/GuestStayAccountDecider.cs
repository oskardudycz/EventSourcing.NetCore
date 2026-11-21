namespace BusinessProcesses.Choreography.GuestStayAccounts;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;

public abstract record GuestStayAccountCommand
{
    public record CheckInGuest(
        Guid GuestStayAccountId,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordCharge(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayAccountId,
        DateTimeOffset Now,
        Guid? GroupCheckoutId = null
    ): GuestStayAccountCommand;
}

public static class GuestStayAccountDecider
{
    public static GuestStayAccountEvent Decide(GuestStayAccountCommand command, GuestStayAccount? guestStayAccount) =>
        (command, guestStayAccount) switch
        {
            (CheckInGuest checkInGuest, null) => CheckIn(checkInGuest),
            (RecordCharge recordCharge, { } state) => RecordCharge(recordCharge, state),
            (RecordPayment recordPayment, { } state) => RecordPayment(recordPayment, state),
            (CheckOutGuest checkOutGuest, { } state) => Checkout(checkOutGuest, state),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };

    public static GuestCheckedIn CheckIn(CheckInGuest command) =>
        new(command.GuestStayAccountId, command.Now);

    public static ChargeRecorded RecordCharge(RecordCharge command, GuestStayAccount state)
    {
        if (state.Status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new ChargeRecorded(state.Id, command.Amount, command.Now);
    }

    public static PaymentRecorded RecordPayment(RecordPayment command, GuestStayAccount state)
    {
        if (state.Status != GuestStayAccountStatus.Open)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new PaymentRecorded(state.Id, command.Amount, command.Now);
    }

    public static GuestStayAccountEvent Checkout(CheckOutGuest command, GuestStayAccount state)
    {
        if (state.Status != GuestStayAccountStatus.Open)
            return new GuestCheckoutFailed(
                state.Id,
                GuestCheckoutFailed.FailureReason.InvalidState,
                command.Now,
                command.GroupCheckoutId
            );

        return state.IsSettled
            ? new GuestCheckedOut(
                state.Id,
                command.Now,
                command.GroupCheckoutId
            )
            : new GuestCheckoutFailed(
                state.Id,
                GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                command.Now,
                command.GroupCheckoutId
            );
    }
}
