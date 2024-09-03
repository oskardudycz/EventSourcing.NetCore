using EntitiesDefinition.Core;
using EntitiesDefinition.Solution1_Aggregates.GuestStayAccounts;
using EntitiesDefinition.Solution1_Aggregates.GroupCheckouts;

namespace EntitiesDefinition.Solution1_Aggregates;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;
using static GroupCheckoutEvent;

public class GuestStayFacade(Database database, EventBus eventBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var account = GuestStayAccount.CheckIn(command.GuestStayId, command.Now);

        await database.Store(command.GuestStayId, account, ct);
        await eventBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordCharge(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account, ct);
        await eventBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordPayment(command.Amount, command.Now);

        await database.Store(command.GuestStayId, account, ct);
        await eventBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.CheckOut(command.Now, command.GroupCheckOutId);

        await database.Store(command.GuestStayId, account, ct);
        await eventBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default) =>
        eventBus.Publish([
            new GroupCheckoutInitiated(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                command.Now
            )
        ], ct);
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
