using HotelManagement.Core;
using HotelManagement.GuestStayAccounts;
using static HotelManagement.Core.Result;

namespace HotelManagement.GuestStay;

public record CheckInGuest(
    Guid GuestStayId
);

public record RecordCharge(
    Guid GuestStayId,
    Decimal Amount
);

public record RecordPayment(
    Guid GuestStayId,
    Decimal Amount
);

public record CheckOutGuest(
    Guid GuestStayId,
    Guid? GroupCheckOutId = null
);

public static class GuestStayDomainService
{
    public static GuestCheckedIn Handle(CheckInGuest command) =>
        new GuestCheckedIn(command.GuestStayId, DateTimeOffset.UtcNow);

    public static ChargeRecorded Handle(GuestStayAccount state, RecordCharge command)
    {
        if (state.Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record charge for not opened account");

        return new ChargeRecorded(state.Id, command.Amount, DateTimeOffset.UtcNow);
    }

    public static PaymentRecorded Handle(GuestStayAccount state, RecordPayment command)
    {
        if (state.Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record payment for not opened account");

        return new PaymentRecorded(state.Id, command.Amount, DateTimeOffset.UtcNow);
    }

    public static Result<GuestCheckedOut, GuestCheckoutFailed> Handle(GuestStayAccount state, CheckOutGuest command)
    {
        if (state.Status != GuestStayAccountStatus.Opened)
            throw new InvalidOperationException("Cannot record payment for not opened account");

        if (!state.IsSettled)
        {
            return Failure<GuestCheckedOut, GuestCheckoutFailed>(
                new GuestCheckoutFailed(
                    state.Id,
                    GuestCheckoutFailed.FailureReason.BalanceNotSettled,
                    DateTimeOffset.UtcNow,
                    command.GroupCheckOutId
                )
            );
        }

        return Success<GuestCheckedOut, GuestCheckoutFailed>(
            new GuestCheckedOut(
                state.Id,
                DateTimeOffset.UtcNow,
                command.GroupCheckOutId
            )
        );
    }
}
