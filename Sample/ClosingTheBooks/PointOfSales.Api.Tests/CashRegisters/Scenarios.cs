using Bogus;

namespace PointOfSales.Api.Tests.CashRegisters;

public static class Scenarios
{
    private static readonly Faker faker = new();

    public static RequestDefinition InitializedCashRegister(string? cashRegisterId = null) =>
        SEND(
            "Initialize Cash Register",
            POST,
            URI($"/api/cash-registers/{cashRegisterId ?? Guid.CreateVersion7().ToString()}")
        );

    public static RequestDefinition OpenedCashierShift(string cashRegisterId, string? cashierId = null) =>
        SEND(
            "Open Cashier Shift",
            POST,
            URI($"/api/cash-registers/{cashRegisterId}/cashier-shifts"),
            BODY(new OpenShiftRequest(cashierId ?? Guid.CreateVersion7().ToString()))
        );

    public static RequestDefinition RegisteredTransaction(Guid cashRegisterId, int shiftNumber, decimal? amount) =>
        SEND(
            "Register Transaction",
            POST,
            URI($"/api/cash-registers/{cashRegisterId}/cashier-shifts/{shiftNumber}/transactions"),
            BODY(new RegisterTransactionRequest(amount ?? faker.Finance.Amount()))
        );

    public static RequestDefinition ClosedCashierShift(
        string cashRegisterId,
        int shiftNumber,
        int etag,
        decimal? declaredTender = null
    ) =>
        SEND(
            "Close Cashier Shift",
            POST,
            URI($"/api/cash-registers/{cashRegisterId}/cashier-shifts/{shiftNumber}/close"),
            BODY(new CloseShiftRequest(declaredTender ?? faker.Finance.Amount())),
            HEADERS(IF_MATCH(etag))
        );
}
