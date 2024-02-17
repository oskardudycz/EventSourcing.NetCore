using Marten.Events.Aggregation;

namespace PointOfSales.CashierShifts;

using static CashierShiftEvent;

public record CurrentCashierShift(
    string Id,
    string CashierRegisterId,
    int ShiftNumber,
    string CashierId,
    decimal Float,
    int TransactionsCount,
    DateTimeOffset StartedAt,
    ClosingDetails? ClosingDetails = null
)
{
    public int Version { get; set; }
}

public record ClosingDetails(
    decimal? DeclaredTender = null,
    decimal? OverageAmount = null,
    decimal? ShortageAmount = null,
    decimal? FinalFloat = null,
    DateTimeOffset? ClosedAt = null
);

public class CurrentCashierShiftProjection: SingleStreamProjection<CurrentCashierShift>
{
    public static CurrentCashierShift Create(ShiftOpened @event) =>
        new(
            @event.CashierShiftId,
            @event.CashierShiftId.CashRegisterId,
            @event.CashierShiftId.ShiftNumber,
            @event.CashierId,
            @event.Float,
            0,
            @event.StartedAt
        );

    public CurrentCashierShift Apply(TransactionRegistered @event, CurrentCashierShift details) =>
        details with { Float = details.Float + @event.Amount, TransactionsCount = details.TransactionsCount + 1 };

    public CurrentCashierShift Apply(ShiftClosed @event, CurrentCashierShift details) =>
        details with
        {
            ClosingDetails = new ClosingDetails(
                @event.DeclaredTender,
                @event.OverageAmount,
                @event.ShortageAmount,
                @event.FinalFloat,
                @event.ClosedAt
            )
        };
}
