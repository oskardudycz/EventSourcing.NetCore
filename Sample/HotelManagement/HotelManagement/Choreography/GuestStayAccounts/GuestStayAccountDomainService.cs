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

public class GuestStayDomainService(IDocumentSession documentSession):
    ICommandHandler<CheckInGuest>,
    ICommandHandler<RecordCharge>,
    ICommandHandler<RecordPayment>,
    ICommandHandler<CheckOutGuest>
{
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
            state => state.RecordCharge(command.Amount, DateTimeOffset.UtcNow),
            GuestStayAccount.Initial,
            ct
        );

    public Task Handle(RecordPayment command,CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            command.ExpectedVersion,
            state => state.RecordPayment(command.Amount, DateTimeOffset.UtcNow),
            GuestStayAccount.Initial,
            ct
        );

    public Task Handle(CheckOutGuest command, CancellationToken ct) =>
        documentSession.GetAndUpdate(
            command.GuestStayId,
            state => state.CheckOut(DateTimeOffset.UtcNow).FlatMap(),
            GuestStayAccount.Initial,
            ct
        );
}
