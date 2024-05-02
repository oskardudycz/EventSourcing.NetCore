using Core.Commands;
using Core.Events;
using Core.Marten.Extensions;
using Core.Structures;
using HotelManagement.Choreography.GuestStayAccounts;
using Marten;
using static Core.Structures.EventOrCommand;

namespace HotelManagement.Choreography.GroupCheckouts;

public record InitiateGroupCheckout(
    Guid GroupCheckoutId,
    Guid ClerkId,
    Guid[] GuestStayIds
);

public record RecordGuestCheckoutsInitiation(
    Guid GroupCheckoutId,
    Guid[] InitiatedGuestStayIds
);

public record RecordGuestCheckoutCompletion(
    Guid GroupCheckoutId,
    Guid GuestStayId,
    DateTimeOffset CompletedAt
);

public record RecordGuestCheckoutFailure(
    Guid GroupCheckoutId,
    Guid GuestStayId,
    DateTimeOffset FailedAt
);

public class GuestStayDomainService(IDocumentSession documentSession, IAsyncCommandBus commandBus)
    :
        ICommandHandler<InitiateGroupCheckout>,
        IEventHandler<GroupCheckoutInitiated>,
        IEventHandler<GuestStayAccounts.GuestCheckedOut>,
        IEventHandler<GuestStayAccounts.GuestCheckoutFailed>
{
    private readonly IAsyncCommandBus commandBus = commandBus;

    public Task Handle(InitiateGroupCheckout command, CancellationToken ct) =>
        documentSession.Add<GroupCheckout>(
            command.GroupCheckoutId.ToString(),
            GroupCheckout.Initiate(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                DateTimeOffset.UtcNow
            ),
            ct
        );

    public Task Handle(GroupCheckoutInitiated @event, CancellationToken ct)
    {
        IEnumerable<EventOrCommand> OnInitiated(GroupCheckout groupCheckout)
        {
            var result = groupCheckout.RecordGuestCheckoutsInitiation(@event.GuestStayIds, @event.InitiatedAt);

            if (result is not null)
            {
                foreach (var guestAccountId in @event.GuestStayIds)
                {
                    yield return Command(new CheckOutGuest(guestAccountId, @event.GroupCheckOutId));
                }

                yield return Event(result);
            }
        }

        return documentSession.GetAndUpdate<GroupCheckout>(@event.GroupCheckOutId.ToString(), OnInitiated, ct);
    }

    public Task Handle(GuestStayAccounts.GuestCheckedOut @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return documentSession.GetAndUpdate<GroupCheckout>(@event.GroupCheckOutId.Value.ToString(),
            groupCheckout => groupCheckout.RecordGuestCheckoutCompletion(@event.GuestStayId, @event.CheckedOutAt),
            ct
        );
    }

    public Task Handle(Choreography.GuestStayAccounts.GuestCheckoutFailed @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return documentSession.GetAndUpdate<GroupCheckout>(@event.GroupCheckOutId.Value.ToString(),
            groupCheckout =>
                groupCheckout.RecordGuestCheckoutFailure(@event.GuestStayId, @event.FailedAt),
            ct
        );
    }
}
