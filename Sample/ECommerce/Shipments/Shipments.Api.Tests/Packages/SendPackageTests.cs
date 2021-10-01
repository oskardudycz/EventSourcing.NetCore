using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Shipments.Packages;
using Shipments.Packages.Events.External;
using Shipments.Packages.Requests;
using Shipments.Products;
using Xunit;
using Xunit.Abstractions;

namespace Shipments.Api.Tests.Packages
{
    public class SendPackageFixture: ApiWithEventsFixture<Startup>
    {
        protected override string ApiUrl => "/api/Shipments";

        public readonly Guid OrderId = Guid.NewGuid();

        public readonly DateTime TimeBeforeSending = DateTime.UtcNow;

        public readonly List<ProductItem> ProductItems = new()
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

        public HttpResponseMessage CommandResponse = default!;

        protected override async Task Setup()
        {
            CommandResponse = await Post(new SendPackage(OrderId, ProductItems));
        }
    }

    public class SendPackageTests: ApiTest<SendPackageFixture>
    {
        public SendPackageTests(SendPackageFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task SendPackage_ShouldReturn_CreatedStatus_With_PackageId()
        {
            var commandResponse = Fixture.CommandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var createdId = await commandResponse.GetResultFromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task SendPackage_ShouldPublish_PackageWasSentEvent()
        {
            var createdId = await Fixture.CommandResponse.GetResultFromJson<Guid>();

            Fixture.PublishedInternalEventsOfType<PackageWasSent>()
                .Should()
                .HaveCount(1)
                .And.Contain(@event =>
                    @event.PackageId == createdId
                    && @event.OrderId == Fixture.OrderId
                    && @event.SentAt > Fixture.TimeBeforeSending
                    && @event.ProductItems.Count == Fixture.ProductItems.Count
                    && @event.ProductItems.All(
                        pi => Fixture.ProductItems.Exists(
                            expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
                );
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task SendPackage_ShouldCreate_Package()
        {
            var createdId = await Fixture.CommandResponse.GetResultFromJson<Guid>();

            // prepare query
            var query = $"{createdId}";

            //send query
            var queryResponse = await Fixture.Get(query);
            queryResponse.EnsureSuccessStatusCode();

            var packageDetails = await queryResponse.GetResultFromJson<Package>();
            packageDetails.Id.Should().Be(createdId);
            packageDetails.OrderId.Should().Be(Fixture.OrderId);
            packageDetails.SentAt.Should().BeAfter(Fixture.TimeBeforeSending);
            packageDetails.ProductItems.Should().NotBeEmpty();
            packageDetails.ProductItems.All(
                pi => Fixture.ProductItems.Exists(
                    expi => expi.ProductId == pi.ProductId && expi.Quantity == pi.Quantity))
                .Should().BeTrue();
        }
    }
}
