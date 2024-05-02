using Core.Marten.Aggregates;
using Marten;

namespace HotelManagement.ProcessManagers.GuestStayAccounts;

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

public class GuestStayDomainService(IDocumentSession documentSession)
{
    public Task Handle(CheckInGuest command, CancellationToken ct) =>
        documentSession.Add(
            command.GuestStayId.ToString(),
            GuestStayAccount.CheckIn(command.GuestStayId, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(RecordCharge command, int expectedVersion, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId.ToString(),
            (GuestStayAccount state) => state.RecordCharge(command.Amount, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(RecordPayment command, int expectedVersion, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId.ToString(),
            (GuestStayAccount state) => state.RecordPayment(command.Amount, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(CheckOutGuest command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId.ToString(),
            (GuestStayAccount state) => state.CheckOut(DateTimeOffset.UtcNow),
            ct
        );
}
