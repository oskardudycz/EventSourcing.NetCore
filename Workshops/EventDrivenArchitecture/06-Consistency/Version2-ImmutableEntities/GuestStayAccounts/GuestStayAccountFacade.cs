using Consistency.Core;

namespace Consistency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class GuestStayAccountFacade
{
    private readonly Database database;
    private readonly MessageBus messageBus;

    public GuestStayAccountFacade(Database database, MessageBus messageBus, BackgroundJobs backgroundJobs)
    {
        this.database = database;
        this.messageBus = messageBus;
    }

    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now);

        await database.Store(command.GuestStayAccountId, GuestStayAccount.Initial.Evolve(@event), 0, ct);
        await messageBus.Publish([@event], ct);
    }

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordCharge(command.Amount, command.Now), ct);

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(command.GuestStayAccountId, account => account.RecordPayment(command.Amount, command.Now), ct);

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var @event = await Retry.OnConflict<GuestStayAccountEvent>(async token =>
        {
            var (account, version) = await database.Get<GuestStayAccount>(command.GuestStayAccountId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            var result = account.Checkout(command.Now, command.GroupCheckoutId);

            if (result is GuestCheckedOut checkedOut)
                await database.Store(command.GuestStayAccountId, account.Evolve(checkedOut), version, token);

            return result;
        }, ct);

        await messageBus.Publish([@event], ct);
    }

    private async ValueTask Handle(
        Guid guestStayAccountId,
        Func<GuestStayAccount, GuestStayAccountEvent> decide,
        CancellationToken ct
    )
    {
        var @event = await Retry.OnConflict(async token =>
        {
            var (account, version) = await database.Get<GuestStayAccount>(guestStayAccountId, token)
                                     ?? throw new InvalidOperationException("Entity not found");

            var @event = decide(account);

            await database.Store(guestStayAccountId, account.Evolve(@event), version, token);

            return @event;
        }, ct);

        await messageBus.Publish([@event], ct);
    }
}

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
