using EntitiesDefinition.Core;
using EntitiesDefinition.Solution2_ImmutableEntities.GroupCheckouts;
using EntitiesDefinition.Solution2_ImmutableEntities.GuestStayAccounts;

namespace EntitiesDefinition.Solution2_ImmutableEntities;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;
using static GroupCheckoutEvent;
using static GroupCheckoutCommand;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now);

        await database.Store(command.GuestStayAccountId, GuestStayAccount.Initial.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordCharge(command.Amount, command.Now);

        await database.Store(command.GuestStayAccountId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordPayment(command.Amount, command.Now);

        await database.Store(command.GuestStayAccountId, account.Evolve(@event), ct);
        await messageBus.Publish([@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        switch (account.Checkout(command.Now, command.GroupCheckoutId))
        {
            case GuestCheckedOut checkedOut:
            {
                await database.Store(command.GuestStayAccountId, account.Evolve(checkedOut), ct);
                await messageBus.Publish([checkedOut], ct);
                return;
            }
            case GuestStayAccountEvent.GuestCheckoutFailed checkOutFailed:
            {
                await messageBus.Publish([checkOutFailed], ct);
                return;
            }
        }
    }

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        messageBus.Publish([
            new GroupCheckoutInitiated(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayAccountIds,
                command.Now
            )
        ], ct);
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

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayAccountIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
