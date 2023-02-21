using Core.Commands;
using HotelManagement.Saga.GuestStayAccounts;

namespace HotelManagement.Saga.GroupCheckouts;

public class GroupCheckoutSaga
{
    private readonly IAsyncCommandBus commandBus;

    public GroupCheckoutSaga(IAsyncCommandBus commandBus) =>
        this.commandBus = commandBus;

    public async Task Handle(GroupCheckoutInitiated @event, CancellationToken ct)
    {
        foreach (var guestAccountId in @event.GuestStayIds)
        {
            await commandBus.Schedule(
                new CheckOutGuest(guestAccountId, @event.GroupCheckoutId),
                ct
            );
        }

        await commandBus.Schedule(
            new RecordGuestCheckoutsInitiation(
                @event.GroupCheckoutId,
                @event.GuestStayIds
            ),
            ct
        );
    }

    public Task Handle(GuestStayAccounts.GuestCheckedOut @event, CancellationToken ct)
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

    public Task Handle(GuestStayAccounts.GuestCheckoutFailed @event, CancellationToken ct)
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
