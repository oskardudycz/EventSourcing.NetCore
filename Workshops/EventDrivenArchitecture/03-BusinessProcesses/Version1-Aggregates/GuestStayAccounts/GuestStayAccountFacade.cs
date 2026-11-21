using BusinessProcesses.Core;

namespace BusinessProcesses.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayAccountFacade(Database database, MessageBus messageBus)
{
    public async ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        var account = GuestStayAccount.CheckIn(command.GuestStayAccountId, command.Now);

        await database.Store(command.GuestStayAccountId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordCharge(command.Amount, command.Now);

        await database.Store(command.GuestStayAccountId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct)
                      ?? throw new InvalidOperationException("Entity not found");

        account.RecordPayment(command.Amount, command.Now);

        await database.Store(command.GuestStayAccountId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
    }

    public async ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        var account = await database.Get<GuestStayAccount>(command.GuestStayAccountId, ct);
        if (account == null)
            return;

        account.Checkout(command.Now, command.GroupCheckoutId);

        await database.Store(command.GuestStayAccountId, account, ct);
        await messageBus.Publish(account.DequeueUncommittedEvents(), ct);
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
