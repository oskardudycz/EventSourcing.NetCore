using HotelManagement.EventStore;

namespace HotelManagement.GuestStayAccounts;

public class GuestStayAccountService(IEventStore eventStore)
{
    private readonly CommandHandler<GuestStayAccount> commandHandler =
        new(GuestStayAccount.Evolve, () => GuestStayAccount.Initial);

    public Task CheckIn(CheckIn command, CancellationToken ct = default)
    {
        var guestStayAccountId = GuestStayAccount.GuestStayAccountId(
            command.GuestStayId, command.RoomId, DateOnly.FromDateTime(command.Now.Date)
        );

        return commandHandler.Handle(eventStore, guestStayAccountId,
            state => [GuestStayAccountDecider.CheckIn(command, state)], ct
        );
    }

    public Task RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        commandHandler.Handle(eventStore, command.GuestStayAccountId,
            state => [GuestStayAccountDecider.RecordCharge(command, state)], ct
        );

    public Task RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        commandHandler.Handle(eventStore, command.GuestStayAccountId,
            state => [GuestStayAccountDecider.RecordPayment(command, state)], ct
        );

    public Task CheckOut(CheckOut command, CancellationToken ct = default) =>
        commandHandler.Handle(eventStore, command.GuestStayAccountId,
            state => [GuestStayAccountDecider.CheckOut(command, state)], ct
        );
}
