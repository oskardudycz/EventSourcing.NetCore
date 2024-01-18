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

public class GuestStayDomainService:
    ICommandHandler<InitiateGroupCheckout>,
    ICommandHandler<RecordGuestCheckoutsInitiation>,
    ICommandHandler<RecordGuestCheckoutCompletion>,
    ICommandHandler<RecordGuestCheckoutFailure>
{
    private readonly IDocumentSession documentSession;

    public GuestStayDomainService(IDocumentSession documentSession) =>
        this.documentSession = documentSession;

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
            (GroupCheckout state) => state.RecordGuestCheckoutsInitiation(command.InitiatedGuestStayIds, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(RecordGuestCheckoutCompletion command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            (GroupCheckout state) => state.RecordGuestCheckoutCompletion(command.GuestStayId, command.CompletedAt),
            ct
        );

    public Task Handle(RecordGuestCheckoutFailure command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            (GroupCheckout state) => state.RecordGuestCheckoutFailure(command.GuestStayId, command.FailedAt),
            ct
        );
}
