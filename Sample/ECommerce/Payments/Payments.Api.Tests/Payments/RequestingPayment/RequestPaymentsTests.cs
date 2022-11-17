using Core.Testing;
using Payments.Api.Requests.Carts;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Payments.Api.Tests.Payments.RequestingPayment;

public class RequestPaymentsTests: IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly ApiSpecification<Program> API;
    private readonly TestWebApplicationFactory<Program> fixture;

    public RequestPaymentsTests(TestWebApplicationFactory<Program> fixture)
    {
        this.fixture = fixture;
        API = ApiSpecification<Program>.Setup(fixture);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task  RequestPayment_ShouldReturn_CreatedStatus_With_PaymentId() =>
        API.Given(
                URI("/api/Payments/"),
                BODY(new RequestPaymentRequest {OrderId = OrderId, Amount = Amount})
            )
            .When(POST)
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1));

    private readonly Guid OrderId = Guid.NewGuid();

    private readonly decimal Amount = new Random().Next(100);
}
