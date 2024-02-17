using Marten;

namespace PointOfSales.CashierShifts;

using static CashierShiftEvent;
using static CashierShiftCommand;
using static CashierShiftDecider;
using CommandResult = (CashierShiftId StreamId, CashierShiftEvent[] Events);

public record CloseAndOpenCommand(
    CashierShiftId CashierShiftId,
    string CashierId,
    decimal DeclaredTender,
    DateTimeOffset Now
);

public static class CloseAndOpenShift
{
    public static (CommandResult, CommandResult) Handle(CloseAndOpenCommand command, CashierShift currentShift)
    {
        var (currentShiftId, cashierId, declaredTender, now) = command;
        var closingResult = Decide(new CloseShift(currentShiftId, declaredTender, now), currentShift);

        currentShift = closingResult.Aggregate(currentShift, (current, @event) => current.Apply(@event));

        var openResult = Decide(new OpenShift(currentShiftId, cashierId, now), currentShift);

        // double check if it was actually
        var opened = openResult.OfType<ShiftOpened>().SingleOrDefault();
        if (opened == null)
            throw new InvalidOperationException("Cannot open new shift!");

        return ((currentShiftId, closingResult), (opened.CashierShiftId, openResult));
    }

    public static async Task<CashierShiftId> CloseAndOpenCashierShift(
        this IDocumentSession documentSession,
        CloseAndOpenCommand command,
        int version,
        CancellationToken ct
    )
    {
        var currentShift =
            await documentSession.Events.AggregateStreamAsync<CashierShift>(command.CashierShiftId, token: ct) ??
            new CashierShift.NonExisting();

        var (closingResult, openResult) = Handle(command, currentShift);

        // Append Closing result to the old stream
        if (closingResult.Events.Length > 0)
            documentSession.Events.Append(closingResult.StreamId, version, closingResult.Events.AsEnumerable());

        if (openResult.Events.Length > 0)
            documentSession.Events.StartStream<CashierShift>(openResult.StreamId, openResult.Events.AsEnumerable());

        await documentSession.SaveChangesAsync(ct);

        return openResult.StreamId;
    }
}
