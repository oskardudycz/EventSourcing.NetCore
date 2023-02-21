using Core.Marten.Extensions;
using Marten;

namespace HotelManagement.Saga.GuestStayAccounts;

public record CheckInGuest(
    Guid GuestStayId
);

public record RecordCharge(
    Guid GuestStayId,
    decimal Amount
);

public record RecordPayment(
    Guid GuestStayId,
    decimal Amount
);

public record CheckOutGuest(
    Guid GuestStayId,
    Guid? GroupCheckOutId = null
);

public class GuestStayDomainService
{
    private readonly IDocumentSession documentSession;

    public GuestStayDomainService(IDocumentSession documentSession) =>
        this.documentSession = documentSession;

    public Task Handle(CheckInGuest command, CancellationToken ct) =>
        documentSession.Add<GuestStayAccount>(
            command.GuestStayId,
            GuestStayAccount.CheckIn(command.GuestStayId, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(RecordCharge command, int expectedVersion, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            expectedVersion,
            (GuestStayAccount state) => state.RecordCharge(command.Amount, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(RecordPayment command, int expectedVersion, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            expectedVersion,
            (GuestStayAccount state) => state.RecordPayment(command.Amount, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(CheckOutGuest command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            (GuestStayAccount state) => state.CheckOut(DateTimeOffset.UtcNow).FlatMap(),
            ct
        );
}
