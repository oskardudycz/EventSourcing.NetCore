namespace PointOfSales.CashRegister;

public static class CashRegisterId
{
    public static string From(string workstation) =>
        $"urn:cash_register:{workstation}";
}

public record CashRegister(string Id)
{
    public static CashRegister Create(CashRegisterInitialized @event) =>
        new(@event.CashRegisterId);
}

public record CashRegisterInitialized(
    string CashRegisterId,
    DateTimeOffset InitializedAt
);
