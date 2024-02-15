using Marten;

namespace PointOfSales.CashierShifts;
using static CashierShiftEvent;

public static class LastCashierShiftLocator
{
    public static async Task<CashierShift> GetLastCashierShift(
        this IDocumentSession documentSession,
        string cashRegisterId
    )
    {
        var tracker = await documentSession.LoadAsync<CashierShiftTracker>(cashRegisterId);

        if (tracker is null)
            throw new InvalidOperationException("Unknown cash register!");

        var lastClosedShiftEvent = tracker.LastShiftClosedSequence.HasValue
            ? (await documentSession.Events.QueryAllRawEvents()
                .SingleAsync(e => e.Sequence == tracker.LastShiftClosedSequence.Value)).Data
            : null;

        return lastClosedShiftEvent is ShiftClosed closed
            ? new CashierShift.Closed(closed.CashierShiftId, closed.FinalFloat)
            : new CashierShift.NonExisting();
    }
}
