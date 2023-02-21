using Core.Commands;
using Core.Events;
using HotelManagement.Choreography.GuestStayAccounts;

namespace HotelManagement.Choreography.GroupCheckouts;

public class GroupCheckoutSaga:
    IEventHandler<GroupCheckoutInitiated>,
    IEventHandler<Sagas.GuestStayAccounts.GuestCheckedOut>,
    IEventHandler<Sagas.GuestStayAccounts.GuestCheckoutFailed>
{
    private readonly IAsyncCommandBus commandBus;

    public GroupCheckoutSaga(IAsyncCommandBus commandBus) =>
        this.commandBus = commandBus;

    public async Task Handle(GroupCheckoutInitiated @event, CancellationToken ct)
    {
        foreach (var guestAccountId in @event.GuestStayIds)
        {
            await commandBus.Schedule(
                new CheckOutGuest(guestAccountId, @event.GroupCheckOutId),
                ct
            );
        }

        await commandBus.Schedule(
            new RecordGuestCheckoutsInitiation(
                @event.GroupCheckOutId,
                @event.GuestStayIds
            ),
            ct
        );
    }

    public Task Handle(Sagas.GuestStayAccounts.GuestCheckedOut @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return commandBus.Schedule(
            new RecordGuestCheckoutCompletion(
                @event.GroupCheckOutId.Value,
                @event.GuestStayId,
                @event.CheckedOutAt
            ),
            ct
        );
    }

    public Task Handle(Sagas.GuestStayAccounts.GuestCheckoutFailed @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return commandBus.Schedule(
            new RecordGuestCheckoutFailure(
                @event.GroupCheckOutId.Value,
                @event.GuestStayId,
                @event.FailedAt
            ),
            ct
        );
    }
}
