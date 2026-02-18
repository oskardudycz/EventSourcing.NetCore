using Core.Commands;
using Core.Marten.Extensions;
using Marten;

namespace HotelManagement.Sagas.GroupCheckouts;

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

public class GuestStayDomainService(IDocumentSession documentSession):
    ICommandHandler<InitiateGroupCheckout>,
    ICommandHandler<RecordGuestCheckoutsInitiation>,
    ICommandHandler<RecordGuestCheckoutCompletion>,
    ICommandHandler<RecordGuestCheckoutFailure>
{
    public Task Handle(InitiateGroupCheckout command, CancellationToken ct) =>
        documentSession.Add<GroupCheckout>(
            command.GroupCheckoutId,
            GroupCheckout.Initiate(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                DateTimeOffset.UtcNow
            ),
            ct
        );

    public Task Handle(RecordGuestCheckoutsInitiation command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GroupCheckoutId,
            state => state.RecordGuestCheckoutsInitiation(command.InitiatedGuestStayIds, DateTimeOffset.UtcNow),
            GroupCheckout.Initial,
            ct
        );

    public Task Handle(RecordGuestCheckoutCompletion command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            state => state.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt),
            GroupCheckout.Initial,
            ct
        );

    public Task Handle(RecordGuestCheckoutFailure command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            state => state.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt),
            GroupCheckout.Initial,
            ct
        );
}
