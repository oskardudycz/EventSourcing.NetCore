using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Shipments.Api.Tests.Core;
using Shipments.Packages;
using Shipments.Packages.Commands;
using Shipments.Packages.Events.External;
using Shipments.Products;
using Xunit;

namespace Shipments.Api.Tests.Packages
{
    public class SendPackageFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl { get; } = "/api/Shipments";

        public readonly Guid OrderId = Guid.NewGuid();

        public readonly DateTime TimeBeforeSending = DateTime.UtcNow;

        public readonly List<ProductItem> ProductItems = new List<ProductItem>
        {
            new ProductItem
            {
                ProductId = Guid.NewGuid(),
                Quantity = 10
            },
            new ProductItem
            {
                ProductId = Guid.NewGuid(),
                Quantity = 3
            }
        };

        public HttpResponseMessage CommandResponse;

        public override async Task InitializeAsync()
        {
            CommandResponse = await PostAsync(new SendPackage {OrderId = OrderId, ProductItems = ProductItems});
        }
    }

    public class SendPackageTests: IClassFixture<SendPackageFixture>
    {
        private readonly SendPackageFixture fixture;

        public SendPackageTests(SendPackageFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_ReservationId()
        {
            var commandResponse = fixture.CommandResponse;
            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var commandResult = await commandResponse.Content.ReadAsStringAsync();
            commandResult.Should().NotBeNull();

            var createdId = commandResult.FromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldPublish_PackageWasSentEvent()
        {
            var createdReservationId = await fixture.CommandResponse.GetResultFromJSON<Guid>();

            fixture.PublishedInternalEventsOfType<PackageWasSent>()
                .Should()
                .HaveCount(1)
                .And.Contain(@event =>
                    @event.PackageId != Guid.Empty
                    && @event.OrderId == fixture.OrderId
                    && @event.SentAt > fixture.TimeBeforeSending
                    && @event.ProductItems.Count == fixture.ProductItems.Count
                    && @event.ProductItems.All(
                        pi => fixture.ProductItems.Exists(
                            expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
                );
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldCreate_Package()
        {
            var createdId = await fixture.CommandResponse.GetResultFromJSON<Guid>();

            // prepare query
            var query = $"{createdId}";

            //send query
            var queryResponse = await fixture.GetAsync(query);
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResult.Should().NotBeNull();

            var reservationDetails = queryResult.FromJson<Package>();
            reservationDetails.Id.Should().Be(createdId);
            reservationDetails.OrderId.Should().Be(fixture.OrderId);
            reservationDetails.SentAt.Should().BeAfter(fixture.TimeBeforeSending);
            reservationDetails.ProductItems.Should().NotBeEmpty();
            reservationDetails.ProductItems.All(
                pi => fixture.ProductItems.Exists(
                    expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
                .Should().BeTrue();
        }
    }
}
