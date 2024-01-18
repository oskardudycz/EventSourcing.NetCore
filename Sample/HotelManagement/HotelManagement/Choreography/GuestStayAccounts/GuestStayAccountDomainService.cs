using Core.Commands;
using Core.Marten.Extensions;
using Marten;

namespace HotelManagement.Choreography.GuestStayAccounts;

public record CheckInGuest(
    Guid GuestStayId
);

public record RecordCharge(
    Guid GuestStayId,
    decimal Amount,
    int ExpectedVersion
);

public record RecordPayment(
    Guid GuestStayId,
    decimal Amount,
    int ExpectedVersion
);

public record CheckOutGuest(
    Guid GuestStayId,
    Guid? GroupCheckOutId = null
);

public class GuestStayDomainService:
    ICommandHandler<CheckInGuest>,
    ICommandHandler<RecordCharge>,
    ICommandHandler<RecordPayment>,
    ICommandHandler<CheckOutGuest>
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

    public Task Handle(RecordCharge command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            command.ExpectedVersion,
            (GuestStayAccount state) => state.RecordCharge(command.Amount, DateTimeOffset.UtcNow),
            ct
        );

    public Task Handle(RecordPayment command,CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            command.ExpectedVersion,
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
