using HotelManagement.Core.Messaging;
using HotelManagement.GuestStay;

namespace HotelManagement.GroupCheckouts;

public class GroupCheckoutSaga
{
    private readonly ICommandBus commandBus;

    public GroupCheckoutSaga(ICommandBus commandBus) =>
        this.commandBus = commandBus;

    public async Task Handle(GroupCheckoutInitiated @event, CancellationToken ct)
    {
        foreach (var guestAccountId in @event.GuestStayIds)
        {
            await commandBus.Send(
                new CheckOutGuest(guestAccountId, @event.GroupCheckoutId),
                ct
            );
        }

        await commandBus.Send(
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

        return commandBus.Send(
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

        return commandBus.Send(
            new RecordGuestCheckoutFailure(
                @event.GroupCheckOutId.Value,
                @event.GuestStayId,
                @event.FailedAt
            ),
            ct
        );
    }
}
