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

    private CashierShiftCommand() { }
}

public static class CashierShiftDecider
{
    public static CashierShiftEvent[] Decide(CashierShiftCommand command, CashierShift state) =>
        (command, state) switch
        {
            (OpenShift open, NonExisting) =>
                [Open(open.CashierId, open.CashRegisterId, 1, 0, open.Now)],

            (OpenShift open, Closed closed) =>
            [
                Open(open.CashierId, open.CashRegisterId, closed.ShiftId.ShiftNumber + 1, closed.FinalFloat, open.Now)
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

    private static ShiftOpened Open(
        string cashierId,
        string cashRegisterId,
        int shiftNumber,
        decimal @float,
        DateTimeOffset now) =>
        new(
            new CashierShiftId(cashRegisterId, shiftNumber),
            cashierId,
            @float,
            now
        );
}



