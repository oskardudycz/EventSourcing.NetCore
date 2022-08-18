using HotelManagement.Core.Marten;
using HotelManagement.GuestStayAccounts;
using Marten;

namespace HotelManagement.GuestStay;

public record CheckInGuest(
    Guid GuestStayId
);

public record RecordCharge(
    Guid GuestStayId,
    Decimal Amount
);

public record RecordPayment(
    Guid GuestStayId,
    Decimal Amount
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
            GuestStayAccount.CheckIn(command.GuestStayId, DateTimeOffset.Now),
            ct
        );

    public Task Handle(RecordCharge command, int expectedVersion, CancellationToken ct) =>
        documentSession.GetAndUpdate<GuestStayAccount>(
            command.GuestStayId,
            expectedVersion,
            state => state.RecordCharge(command.Amount, DateTimeOffset.Now),
            ct
        );

    public Task Handle(RecordPayment command, int expectedVersion, CancellationToken ct) =>
        documentSession.GetAndUpdate<GuestStayAccount>(
            command.GuestStayId,
            expectedVersion,
            state => state.RecordPayment(command.Amount, DateTimeOffset.Now),
            ct
        );

    public Task Handle(CheckOutGuest command, CancellationToken ct) =>
        documentSession.GetAndUpdate<GuestStayAccount>(
            command.GuestStayId,
            state => state.CheckOut(DateTimeOffset.Now)
                .Map<object>(success => success, failed => failed),
            ct
        );
}
