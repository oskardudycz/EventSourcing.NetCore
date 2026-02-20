namespace HotelManagement.GuestStayAccounts;

public record CheckIn(
    string GuestStayId,
    string RoomId,
    DateTimeOffset Now
);

public record RecordCharge(
    string GuestStayAccountId,
    decimal Amount,
    DateTimeOffset Now
);

public record RecordPayment(
    string GuestStayAccountId,
    decimal Amount,
    DateTimeOffset Now
);

public record CheckOut(
    string GuestStayAccountId,
    DateTimeOffset Now
);

public static class GuestStayAccountDecider
{
    public static GuestCheckedIn CheckIn(CheckIn command, GuestStayAccount state) =>
        new GuestCheckedIn(
            GuestStayAccount.GuestStayAccountId(
                command.GuestStayId,
                command.RoomId,
                DateOnly.FromDateTime(command.Now.Date)
            ),
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

    public static object CheckOut(CheckOut command, GuestStayAccount state)
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
