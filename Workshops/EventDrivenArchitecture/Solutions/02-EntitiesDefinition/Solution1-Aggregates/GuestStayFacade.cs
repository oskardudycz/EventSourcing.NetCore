using EntitiesDefinition.Core;

namespace EntitiesDefinition.Solution1_Aggregates;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayFacade(Database database, EventBus eventBus)
{
    public ValueTask CheckInGuest(CheckInGuest command, CancellationToken ct = default)
    {
        // TODO: Fill the implementation calling your entity/aggregate
        throw new NotImplementedException("TODO: Fill the implementation calling your entity/aggregate");
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        // TODO: Fill the implementation calling your entity/aggregate, for instance:
        var entity = await database.Get<object>(command.GuestStayId, ct) // use here your real type
                     ?? throw new InvalidOperationException("Entity not found");

        // entity.DoSomething();

        object[] events = [new object()]; // get the new event(s) as an outcome of the business logic

        await database.Store(command.GuestStayId, entity, ct);
        await eventBus.Publish(events, ct);

        throw new NotImplementedException("TODO: Fill the implementation calling your entity/aggregate");
    }

    public ValueTask RecordPayment(RecordPayment command, CancellationToken ct = default)
    {
        // TODO: Fill the implementation calling your entity/aggregate
        throw new NotImplementedException("TODO: Fill the implementation calling your entity/aggregate");
    }

    public ValueTask CheckOutGuest(CheckOutGuest command, CancellationToken ct = default)
    {
        // TODO: Fill the implementation calling your entity/aggregate
        throw new NotImplementedException();
    }

    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        // TODO: Fill the implementation calling your entity/aggregate
        throw new NotImplementedException();
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
