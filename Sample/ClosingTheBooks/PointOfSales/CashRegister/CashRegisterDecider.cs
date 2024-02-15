namespace PointOfSales.CashRegister;

public record InitializeCashRegister(
    string CashRegisterId,
    DateTimeOffset Now
);

public static class CashRegisterDecider
{
    public static object[] Decide(InitializeCashRegister command) =>
        [new CashRegisterInitialized(command.CashRegisterId, command.Now)];
}
