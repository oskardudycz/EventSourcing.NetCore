namespace PointOfSales.CashierShifts;

using static CashierShiftCommand;
using static CashierShiftEvent;
using static CashierShift;

public abstract record CashierShiftCommand
{
    public record OpenShift(
        string CashRegisterId,
        string CashierId,
        DateTimeOffset Now
    ): CashierShiftCommand;

    public record RegisterTransaction(
        CashierShiftId CashierShiftId,
        string TransactionId,
        decimal Amount,
        DateTimeOffset Now
    ): CashierShiftCommand;

    public record CloseShift(
        CashierShiftId CashierShiftId,
        decimal DeclaredTender,
        DateTimeOffset Now
    ): CashierShiftCommand;
}

public static class CashierShiftDecider
{
    public static object[] Decide(CashierShiftCommand command, CashierShift state) =>
        (command, state) switch
        {
            (OpenShift open, NonExisting) =>
            [
                new ShiftOpened(
                    new CashierShiftId(open.CashRegisterId, 1),
                    open.CashierId,
                    0,
                    open.Now
                )
            ],

            (OpenShift open, Closed closed) =>
            [

                new ShiftOpened(
                    new CashierShiftId(open.CashRegisterId, closed.ShiftId.ShiftNumber + 1),
                    open.CashierId,
                    closed.FinalFloat,
                    open.Now
                )
            ],

            (OpenShift, Opened) => [],

            (RegisterTransaction registerTransaction, Opened openShift) =>
            [
                new TransactionRegistered(
                    openShift.ShiftId,
                    registerTransaction.TransactionId,
                    registerTransaction.Amount,
                    registerTransaction.Now
                )
            ],

            (CloseShift close, Opened openShift) =>
            [
                new ShiftClosed(
                    openShift.ShiftId,
                    close.DeclaredTender,
                    close.DeclaredTender - openShift.Float,
                    openShift.Float - close.DeclaredTender,
                    openShift.Float,
                    close.Now
                )
            ],
            (CloseShift, Closed) => [],

            _ => throw new InvalidOperationException($"Cannot run {command.GetType().Name} on {state.GetType().Name}")
        };
}
