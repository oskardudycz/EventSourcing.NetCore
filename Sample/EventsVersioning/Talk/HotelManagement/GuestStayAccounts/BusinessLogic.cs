namespace HotelManagement.GuestStayAccounts;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;

public abstract record GuestStayAccountCommand
{
    public record CheckIn(
        string ClerkId,
        string GuestStayId,
        string RoomId,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordCharge(
        string GuestStayAccountId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        string GuestStayAccountId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOut(
        string ClerkId,
        string GuestStayAccountId,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    private GuestStayAccountCommand() { }
}

public static class GuestStayAccountDecider
{
    public static GuestCheckedIn CheckIn(CheckIn command, GuestStayAccount state) =>
        new GuestCheckedIn(
            $"{command.GuestStayId}:{command.RoomId}:{command.Now.Date:yyyy-MM-dd}",
            command.GuestStayId,
            command.RoomId,
            command.ClerkId,
            command.Now
        );

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

    public static GuestStayAccountEvent CheckOut(CheckOut command, GuestStayAccount state)
    {
        if (state.Status != GuestStayAccountStatus.Opened)
            return new GuestCheckoutFailed(
                state.Id,
                command.ClerkId,
                GuestCheckoutFailed.FailureReason.NotOpened,
                command.Now
            );

        return state.IsSettled
            ? new GuestCheckedOut(
                state.Id,
                command.ClerkId,
                command.Now
            )
            : new GuestCheckoutFailed(
                state.Id,
                command.ClerkId,
                GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                command.Now
            );
    }
}
