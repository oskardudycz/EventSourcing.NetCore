using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Payments.Api.Requests.Carts;
using Payments.Payments.RequestingPayment;
using Xunit;

namespace Payments.Api.Tests.Payments.RequestingPayment
{
    public class RequestPaymentsTestsFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl => "/api/Payments";

        public readonly Guid OrderId = Guid.NewGuid();

        public readonly decimal Amount = new Random().Next(100);

        public readonly DateTime TimeBeforeSending = DateTime.UtcNow;

        public HttpResponseMessage CommandResponse = default!;

        public override async Task InitializeAsync()
        {
            CommandResponse = await Post(new RequestPaymentRequest {OrderId = OrderId, Amount = Amount});
        }
    }

    public class RequestPaymentsTestsTests: IClassFixture<RequestPaymentsTestsFixture>
    {
        private readonly RequestPaymentsTestsFixture fixture;

        public RequestPaymentsTestsTests(RequestPaymentsTestsFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_PaymentId()
        {
            var commandResponse = fixture.CommandResponse;
            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var createdId = await commandResponse.GetResultFromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldPublish_PaymentInitializedEvent()
        {
            var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();

            fixture.PublishedInternalEventsOfType<PaymentRequested>()
                .Should()
                .HaveCount(1)
                .And.Contain(@event =>
                    @event.PaymentId == createdId
                    && @event.OrderId == fixture.OrderId
                    && @event.Amount == fixture.Amount
                );
        }

        // [Fact]
        // [Trait("Category", "Exercise")]
        // public async Task CreateCommand_ShouldCreate_Payment()
        // {
        //     var createdId = await fixture.CommandResponse.GetResultFromJSON<Guid>();
        //
        //     // prepare query
        //     var query = $"{createdId}";
        //
        //     //send query
        //     var queryResponse = await fixture.GetAsync(query);
        //     queryResponse.EnsureSuccessStatusCode();
        //
        //     var queryResult = await queryResponse.Content.ReadAsStringAsync();
        //     queryResult.Should().NotBeNull();
        //
        //     var paymentDetails = queryResult.FromJson<Payment>();
        //     paymentDetails.Id.Should().Be(createdId);
        //     paymentDetails.OrderId.Should().Be(fixture.ClientId);
        //     paymentDetails.SentAt.Should().BeAfter(fixture.TimeBeforeSending);
        //     paymentDetails.ProductItems.Should().NotBeEmpty();
        //     paymentDetails.ProductItems.All(
        //         pi => fixture.ProductItems.Exists(
        //             expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
        //         .Should().BeTrue();
        // }
    }
}
