namespace BusinessProcesses.ToDoList.GuestStayAccounts;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;

public abstract record GuestStayAccountCommand
{
    public record CheckInGuest(
        Guid GuestStayId,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordCharge(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayId,
        DateTimeOffset Now,
        Guid? GroupCheckOutId = null
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
            (CheckOutGuest checkOutGuest, { } state) => CheckOut(checkOutGuest, state),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };

    public static GuestCheckedIn CheckIn(CheckInGuest command) =>
        new(command.GuestStayId, command.Now);

    public static ChargeRecorded RecordCharge(RecordCharge command, GuestStayAccount state)
    {
        if (state.Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new ChargeRecorded(state.Id, command.Amount, command.Now);
    }

    public static PaymentRecorded RecordPayment(RecordPayment command, GuestStayAccount state)
    {
        if (state.Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new PaymentRecorded(state.Id, command.Amount, command.Now);
    }

    public static GuestStayAccountEvent CheckOut(CheckOutGuest command, GuestStayAccount state)
    {
        if (state.Status != GuestStayAccountStatus.Opened)
            return new GuestCheckOutFailed(
                state.Id,
                GuestCheckOutFailed.FailureReason.NotOpened,
                command.Now,
                command.GroupCheckOutId
            );

        return state.IsSettled
            ? new GuestCheckedOut(
                state.Id,
                command.Now,
                command.GroupCheckOutId
            )
            : new GuestCheckOutFailed(
                state.Id,
                GuestCheckOutFailed.FailureReason.BalanceNotSettled,
                command.Now,
                command.GroupCheckOutId
            );
    }
}
