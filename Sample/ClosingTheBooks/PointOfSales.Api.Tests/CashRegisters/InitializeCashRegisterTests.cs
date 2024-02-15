namespace PointOfSales.Api.Tests.CashRegisters;

public class InitializeCashRegisterTests(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Fact]
    public Task InitializeCashRegister() =>
        api.Given()
            .When(
                POST,
                URI($"/api/cash-registers/{cashRegisterId}")
            ).Then(CREATED_WITH_DEFAULT_HEADERS($"/api/cash-registers/{cashRegisterId}"));

    private readonly Guid cashRegisterId = Guid.NewGuid();
}
