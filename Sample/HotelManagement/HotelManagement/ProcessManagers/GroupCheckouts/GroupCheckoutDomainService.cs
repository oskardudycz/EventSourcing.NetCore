using Core.Marten.ProcessManagers;
using Marten;

namespace HotelManagement.ProcessManagers.GroupCheckouts;

public record InitiateGroupCheckout(
    Guid GroupCheckoutId,
    Guid ClerkId,
    Guid[] GuestStayIds
);

public class GuestStayDomainService
{
    private readonly IDocumentSession documentSession;

    public GuestStayDomainService(IDocumentSession documentSession) =>
        this.documentSession = documentSession;

    public Task Handle(InitiateGroupCheckout command, CancellationToken ct) =>
        documentSession.Add(
            command.GroupCheckoutId.ToString(),
            GroupCheckoutProcessManager.Initiate(
                command.GroupCheckoutId,
                command.ClerkId,
                command.GuestStayIds,
                DateTimeOffset.UtcNow
            ),
            ct
        );

    public Task Handle(GroupCheckoutInitiated @event, CancellationToken ct) =>
        documentSession.GetAndUpdate<GroupCheckoutProcessManager>(
            @event.GroupCheckOutId.ToString(),
            processManager => processManager.Handle(@event),
            ct
        );

    public Task Handle(GuestStayAccounts.GuestCheckedOut @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return documentSession.GetAndUpdate<GroupCheckoutProcessManager>(
            @event.GroupCheckOutId.Value.ToString(),
            processManager => processManager.Handle(@event),
            ct
        );
    }

    public Task Handle(GuestStayAccounts.GuestCheckoutFailed @event, CancellationToken ct)
    {
        if (!@event.GroupCheckOutId.HasValue)
            return Task.CompletedTask;

        return documentSession.GetAndUpdate<GroupCheckoutProcessManager>(
            @event.GroupCheckOutId.Value.ToString(),
            processManager => processManager.Handle(@event),
            ct
        );
    }

}
