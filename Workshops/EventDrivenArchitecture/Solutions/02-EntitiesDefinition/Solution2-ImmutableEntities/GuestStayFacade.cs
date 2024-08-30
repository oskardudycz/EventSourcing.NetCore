using EntitiesDefinition.Core;
using EntitiesDefinition.Solution2_ImmutableEntities.GroupCheckouts;
using EntitiesDefinition.Solution2_ImmutableEntities.GuestStayAccounts;

namespace EntitiesDefinition.Solution2_ImmutableEntities;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;
using static GroupCheckoutEvent;
using static GroupCheckoutCommand;

public class GuestStayFacade(Database database, EventBus eventBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var @event = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, GuestStayAccount.Initial.Evolve(@event), ct);
        await eventBus.Publish([@event], ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordCharge(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventBus.Publish([@event], ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        var @event = account.RecordPayment(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account.Evolve(@event), ct);
        await eventBus.Publish([@event], ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        switch (account.CheckOut(command.Now, command.GroupCheckOutId))
        {
            case ({ } checkedOut, _):
            {
                await database.Store(command.GuestStayId, account.Evolve(checkedOut), ct);
                await eventBus.Publish([checkedOut], ct);
                return;
            }
            case (_, { } checkOutFailed):
            {
                await eventBus.Publish([checkOutFailed], ct);
                return;
            }
        }
    }

    public async ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        var @event =
            GroupCheckout.Initiate(command.GroupCheckoutId, command.ClerkId, command.GuestStayIds, command.Now);

        await database.Store(command.GroupCheckoutId, GroupCheckout.Initial.Evolve(@event), ct);
        await eventBus.Publish([@event], ct);
    }
}

public abstract record GuestStayAccountCommand
{
    public record CheckInGuest(
        Guid GuestStayId,
        DateTimeOffset Now
    );

    public record RecordCharge(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset Now
    );

    public record RecordPayment(
        Guid GuestStayId,
        decimal Amount,
        DateTimeOffset Now
    );

    public record CheckOutGuest(
        Guid GuestStayId,
        DateTimeOffset Now,
        Guid? GroupCheckOutId = null
    );
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
