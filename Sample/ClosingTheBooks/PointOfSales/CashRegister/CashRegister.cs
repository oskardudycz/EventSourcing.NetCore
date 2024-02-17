namespace PointOfSales.CashRegister;

public record CashRegister(string Id)
{
    public static CashRegister Create(CashRegisterInitialized @event) =>
        new(@event.CashRegisterId);
}

public record CashRegisterInitialized(
    string CashRegisterId,
    DateTimeOffset InitializedAt
);
