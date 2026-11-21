using OptimisticConcurrency.RetryInInfrastructure.Core;
using OptimisticConcurrency.Core;

namespace OptimisticConcurrency.RetryInInfrastructure.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;

public class GuestStayFacade(Database database, MessageBus messageBus)
{
    private readonly CommandHandler<GuestStayAccount, GuestStayAccountEvent> commandHandler =
        new(database, GuestStayAccount.Initial, (state, @event) => state.Evolve(@event));

    public ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayId,
            account => account.Status == GuestStayAccountStatus.NotExisting
                ? [GuestStayAccount.CheckIn(command.GuestStayId, command.Now)]
                : [],
            ct
        );

    public ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayId,
            account =>
            {
                if (account.Status == GuestStayAccountStatus.NotExisting)
                    throw new InvalidOperationException("Entity not found");

                return account.RecordCharge(command.ChargeId, command.Amount, command.Now);
            },
            ct
        );

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayId,
            account =>
            {
                if (account.Status == GuestStayAccountStatus.NotExisting)
                    throw new InvalidOperationException("Entity not found");

                return account.RecordPayment(command.PaymentId, command.Amount, command.Now);
            },
            ct
        );

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default) =>
        Handle(
            command.GuestStayId,
            account => account.Status == GuestStayAccountStatus.NotExisting
                ? []
                : account.CheckOut(command.Now, command.GroupCheckOutId),
            ct
        );

    private async ValueTask Handle(
        Guid guestStayId,
        Func<GuestStayAccount, GuestStayAccountEvent[]> handle,
        CancellationToken ct
    )
    {
        var events = await commandHandler.Handle(guestStayId, handle, ct);

        if (events.Length == 0)
            return;

        await messageBus.Publish(events.Cast<object>().ToArray(), ct);
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
        Guid ChargeId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record RecordPayment(
        Guid GuestStayId,
        Guid PaymentId,
        decimal Amount,
        DateTimeOffset Now
    ): GuestStayAccountCommand;

    public record CheckOutGuest(
        Guid GuestStayId,
        DateTimeOffset Now,
        Guid? GroupCheckOutId = null
    ): GuestStayAccountCommand;
}
