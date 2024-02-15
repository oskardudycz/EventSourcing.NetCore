using System.Text.Json.Serialization;

namespace PointOfSales.CashierShifts;

using System;
using static CashierShiftEvent;

public record CashierShift
{
    public record NonExisting: CashierShift;

    public record Opened(
        CashierShiftId ShiftId,
        // The amount of money in a cash register or till before and after a person's shift
        decimal Float
    ): CashierShift;

    public record Closed(
        CashierShiftId ShiftId,
        decimal FinalFloat
    ): CashierShift;

    public CashierShift Apply(CashierShiftEvent @event) =>
        (this, @event) switch
        {
            (NonExisting or Closed , ShiftOpened shiftOpened) =>
                new Opened(shiftOpened.CashierShiftId, shiftOpened.Float),

            (Opened state, TransactionRegistered transactionRegistered) =>
                state with { Float = state.Float + transactionRegistered.Amount },

            (Opened state, ShiftClosed shiftClosed) =>
                new Closed(state.ShiftId, shiftClosed.FinalFloat),

            _ => this
        };

    private CashierShift() { }

    public string Id { get; init; } = default!;
}

public record CashierShiftId(string CashRegisterId, int ShiftNumber)
{
    public static implicit operator string(CashierShiftId id) => id.ToString();

    public override string ToString() => $"urn:cashier_shift:{CashRegisterId}:{ShiftNumber}";
}

public abstract record CashierShiftEvent(CashierShiftId CashierShiftId)
{
    public record ShiftOpened(
        CashierShiftId CashierShiftId,
        string CashierId,
        decimal Float,
        DateTimeOffset StartedAt
    ): CashierShiftEvent(CashierShiftId);

    public record TransactionRegistered(
        CashierShiftId CashierShiftId,
        string TransactionId,
        decimal Amount,
        DateTimeOffset RegisteredAt
    ): CashierShiftEvent(CashierShiftId);

    public record ShiftClosed(
        CashierShiftId CashierShiftId,
        decimal DeclaredTender,
        decimal OverageAmount,
        decimal ShortageAmount,
        decimal FinalFloat,
        DateTimeOffset ClosedAt
    ): CashierShiftEvent(CashierShiftId);
}
