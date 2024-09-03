using BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GuestStayAccounts;

namespace BusinessProcesses.Version2_ImmutableEntities.ProcessManagers.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;
using static GuestStayAccountEvent;
using static SagaResult;

public static class GroupCheckoutSaga
{
    public static Command<CheckOutGuest>[] Handle(GroupCheckoutInitiated @event) =>
        @event.GuestStayIds.Select(guestAccountId =>
            Send(new CheckOutGuest(guestAccountId, @event.InitiatedAt, @event.GroupCheckoutId))
        ).ToArray();

    public static SagaResult Handle(GuestCheckedOut @event)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Ignore;

        return Send(
            new RecordGuestCheckoutCompletion(
                @event.GroupCheckOutId.Value,
                @event.GuestStayId,
                @event.CheckedOutAt
            )
        );
    }

    public static SagaResult Handle(GuestStayAccountEvent.GuestCheckoutFailed @event)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Ignore;

        return Send(
            new RecordGuestCheckoutFailure(
                @event.GroupCheckOutId.Value,
                @event.GuestStayId,
                @event.FailedAt
            )
        );
    }
};

public abstract record SagaResult
{
    public record Command<T>(T Message): SagaResult;

    public record Event<T>(T Message): SagaResult;

    public record None: SagaResult;

    public static Command<T> Send<T>(T command) => new(command);

    public static Event<T> Publish<T>(T @event) => new(@event);

    public static readonly None Ignore = new();
}
