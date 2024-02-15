using System.Net;

namespace PointOfSales.Api.Tests.CashRegisters;

using static Scenarios;

public class EndToEndPositiveScenarios(ApiSpecification<Program> api):
    IClassFixture<ApiSpecification<Program>>
{
    [Fact]
    public Task ForRegisteredCashRegister_OpensFirstCashierShift() =>
        api.Given([InitializedCashRegister(cashRegisterId)])
            .When(
                POST,
                URI($"/api/cash-registers/{cashRegisterId}/cashier-shifts"),
                BODY(new OpenShiftRequest(Guid.NewGuid().ToString()))
            ).Then(CREATED_WITH_DEFAULT_HEADERS($"/api/cash-registers/{cashRegisterId}/cashier-shifts/1"));

    [Fact]
    public Task ForOpenedCashRegister_DoesntOpensNextCashierShift() =>
        api.Given([InitializedCashRegister(cashRegisterId), OpenedCashierShift(cashRegisterId)])
            .When(
                POST,
                URI($"/api/cash-registers/{cashRegisterId}/cashier-shifts"),
                BODY(new OpenShiftRequest(Guid.NewGuid().ToString()))
            ).Then(PRECONDITION_FAILED);

    [Fact]
    public Task ForOpenedCashRegister_ClosesCashierShift() =>
        api.Given([
                InitializedCashRegister(cashRegisterId),
                OpenedCashierShift(cashRegisterId)
            ])
            .When(
                POST,
                URI($"/api/cash-registers/{cashRegisterId}/cashier-shifts/1/close"),
                BODY(new CloseShiftRequest(100)),
                HEADERS(IF_MATCH(1))
            ).Then(OK);


    [Fact]
    public Task ForClosedCashRegister_OpensCashierShift() =>
        api.Given([
                InitializedCashRegister(cashRegisterId),
                OpenedCashierShift(cashRegisterId),
                ClosedCashierShift(cashRegisterId, 1, 1)
            ])
            .When(
                POST,
                URI($"/api/cash-registers/{cashRegisterId}/cashier-shifts"),
                BODY(new OpenShiftRequest(Guid.NewGuid().ToString()))
            ).Then(CREATED_WITH_DEFAULT_HEADERS($"/api/cash-registers/{cashRegisterId}/cashier-shifts/2"));

    private readonly string cashRegisterId = Guid.NewGuid().ToString();
}
