using EntitiesDefinition.Core;

namespace EntitiesDefinition;

using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class GuestStayFacade(EventStore eventStore)
{
    public ValueTask CheckInGuest(CheckInGuest command)
    {
        // TODO: Fill the implementation calling your entity/aggregate
        throw new NotImplementedException("TODO: Fill the implementation calling your entity/aggregate");
    }

    public async ValueTask RecordCharge(RecordCharge command, CancellationToken ct = default)
    {
        // TODO: Fill the implementation calling your entity/aggregate, for instance:
        // var entity = await eventStore.AggregateStream<GuestStayAccount>(command.GuestStayId, ct) // use here your real type
        //              ?? throw new InvalidOperationException("Entity not found");
        //
        // entity.DoSomething();
        //
        // object[] events = [new object()]; // get the new event(s) as an outcome of the business logic
        //
        // await eventStore.AppendToStream(entity.Id, events, ct);

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
