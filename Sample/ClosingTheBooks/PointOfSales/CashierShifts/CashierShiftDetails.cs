using Marten.Events.Aggregation;

namespace PointOfSales.CashierShifts;

using static CashierShiftEvent;

public record CashierShiftDetails(
    string Id,
    string CashierRegisterId,
    int ShiftNumber,
    string CashierId,
    decimal Float,
    int TransactionsCount,
    DateTimeOffset StartedAt,
    ClosingDetails? ClosingDetails = null
);

public record ClosingDetails(
    decimal? DeclaredTender = null,
    decimal? OverageAmount = null,
    decimal? ShortageAmount = null,
    decimal? FinalFloat = null,
    DateTimeOffset? ClosedAt = null
);

public class CashierShiftDetailsProjection: SingleStreamProjection<CashierShiftDetails>
{
    public static CashierShiftDetails Create(ShiftOpened @event) =>
        new(
            @event.CashierShiftId,
            @event.CashierShiftId.CashRegisterId,
            @event.CashierShiftId.ShiftNumber,
            @event.CashierId,
            @event.Float,
            0,
            @event.StartedAt
        );

    public CashierShiftDetails Apply(TransactionRegistered @event, CashierShiftDetails details) =>
        details with { Float = details.Float + @event.Amount, TransactionsCount = details.TransactionsCount + 1 };

    public CashierShiftDetails Apply(ShiftClosed @event, CashierShiftDetails details) =>
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
