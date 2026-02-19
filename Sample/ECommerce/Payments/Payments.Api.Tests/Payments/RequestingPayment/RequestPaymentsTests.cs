using Core.Testing;
using Payments.Api.Requests.Carts;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Payments.Api.Tests.Payments.RequestingPayment;

public class RequestPaymentsTests
{
    private readonly ApiSpecification<Program> API = ApiSpecification<Program>.Setup(
        new TestWebApplicationFactory<Program>()
    );

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task RequestPayment_ShouldReturn_CreatedStatus_With_PaymentId() =>
        API.Given()
            .When(
                POST,
                URI("/api/Payments/"),
                BODY(new RequestPaymentRequest { OrderId = OrderId, Amount = Amount })
            )
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1));

    private readonly Guid OrderId = Guid.CreateVersion7();

    private readonly decimal Amount = new Random().Next(100);
}
