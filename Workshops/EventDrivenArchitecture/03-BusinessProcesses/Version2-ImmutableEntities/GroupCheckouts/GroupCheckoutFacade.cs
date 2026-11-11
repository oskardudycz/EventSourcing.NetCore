using BusinessProcesses.Core;

namespace BusinessProcesses.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;

public class GroupCheckoutFacade(Database database, MessageBus messageBus)
{
    public ValueTask InitiateGroupCheckout(InitiateGroupCheckout command, CancellationToken ct = default)
    {
        // TODO: Fill the implementation calling your entity/aggregate
        throw new NotImplementedException("TODO: Fill the implementation calling your entity/aggregate");
    }

    public async ValueTask RecordGuestCheckoutCompletion(
        RecordGuestCheckoutCompletion command,
        CancellationToken ct = default
    )
    {
        // TODO: Fill the implementation calling your entity/aggregate, for instance:
        var groupCheckout = await database.Get<GroupCheckout>(command.GroupCheckoutId, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        // groupCheckout.DoSomething();

        object[] events = []; // get the new event(s) as an outcome of the business logic

        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await messageBus.Publish(events, ct);

        throw new NotImplementedException("TODO: Fill the implementation calling your entity/aggregate");
    }

    public async ValueTask RecordGuestCheckoutFailure(
        RecordGuestCheckoutFailure command,
        CancellationToken ct = default
    )
    {
        // TODO: Fill the implementation calling your entity/aggregate, for instance:
        var groupCheckout = await database.Get<GroupCheckout>(command.GroupCheckoutId, ct)
                            ?? throw new InvalidOperationException("Entity not found");

        // groupCheckout.DoSomething();

        object[] events = []; // get the new event(s) as an outcome of the business logic

        await database.Store(command.GroupCheckoutId, groupCheckout, ct);
        await messageBus.Publish(events, ct);

        throw new NotImplementedException("TODO: Fill the implementation calling your entity/aggregate");
    }
}

public abstract record GroupCheckoutCommand
{
    public record InitiateGroupCheckout(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset Now
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutCompletion(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutCommand;

    public record RecordGuestCheckoutFailure(
        Guid GroupCheckoutId,
        Guid GuestStayId,
        DateTimeOffset FailedAt
    ): GroupCheckoutCommand;

    private GroupCheckoutCommand() { }
}
