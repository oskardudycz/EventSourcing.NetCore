using Consistency.Core;

namespace Consistency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayFacade
{
    private readonly Database database;
    private readonly MessageBus messageBus;

    public GuestStayFacade(Database database, MessageBus messageBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;
        this.messageBus = messageBus;
    }

    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), 0, ct);
        await messageBus.Publish([@event], ct);
    }

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordCharge(command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayId, account => account.RecordPayment(command.Amount, command.Now), ct);

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var @event = await Retry.OnConflict<GuestStayAccountEvent>(async token =>
        {
            var (account, version) = await database.Get<GuestStayAccount>(command.GuestStayId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            var result = account.CheckOut(command.Now, command.GroupCheckOutId);

            if (result is GuestCheckedOut checkedOut)
                await database.Store(command.GuestStayId, account.Evolve(checkedOut), version, token);

            return result;
        }, ct);

        await messageBus.Publish([@event], ct);
    }

    private async ValueTask Handle(
        Guid guestStayId,
        Func<GuestStayAccount, GuestStayAccountEvent> decide,
        CancellationToken ct
    )
    {
        var @event = await Retry.OnConflict(async token =>
        {
            var (account, version) = await database.Get<GuestStayAccount>(guestStayId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            var @event = decide(account);

            await database.Store(guestStayId, account.Evolve(@event), version, token);

            return @event;
        }, ct);

        await messageBus.Publish([@event], ct);
    }
}

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
