using Marten.Events;
using Marten.Events.Projections;
using PointOfSales.CashRegister;

namespace PointOfSales.CashierShifts;

public record CashierShiftTracker(
    string Id, // Cash Register Id
    long? LastShiftClosedSequence
);

public class CashierShiftTrackerProjection: MultiStreamProjection<CashierShiftTracker, string>
{
    public CashierShiftTrackerProjection()
    {
        Identity<CashRegisterInitialized>(e => e.CashRegisterId);
        Identity<CashierShiftEvent.ShiftClosed>(e => e.CashierShiftId.CashRegisterId);
    }

    public CashierShiftTracker Create(CashRegisterInitialized initialized) =>
        new(initialized.CashRegisterId, null);

    public CashierShiftTracker Apply(IEvent<CashierShiftEvent.ShiftClosed> closed, CashierShiftTracker current) =>
        current with { LastShiftClosedSequence = closed.Sequence };
}
