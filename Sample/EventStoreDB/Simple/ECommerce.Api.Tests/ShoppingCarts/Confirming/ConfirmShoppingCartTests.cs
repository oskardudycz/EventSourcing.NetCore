using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using ECommerce.Api.Requests;
using FluentAssertions;
using Xunit;

namespace ECommerce.Api.Tests.ShoppingCarts.Confirming
{
    public class ConfirmShoppingCartFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl => "/api/ShoppingCarts";

        public Guid CartId { get; private set; }

        public readonly Guid ClientId = Guid.NewGuid();

        public HttpResponseMessage CommandResponse = default!;

        public override async Task InitializeAsync()
        {
            var initializeResponse = await Post(new InitializeShoppingCartRequest(ClientId));
            initializeResponse.EnsureSuccessStatusCode();

            CartId = await initializeResponse.GetResultFromJson<Guid>();

            CommandResponse = await Put($"{CartId}/confirmation");
        }
    }

    public class ConfirmShoppingCartTests: IClassFixture<ConfirmShoppingCartFixture>
    {
        private readonly ConfirmShoppingCartFixture fixture;

        public ConfirmShoppingCartTests(ConfirmShoppingCartFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public Task ConfirmCommand_ShouldReturn_OK()
        {
            var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            return Task.CompletedTask;
        }

        // [Fact]
        // [Trait("Category", "Acceptance")]
        // public async Task CreateCommand_ShouldCreate_Cart()
        // {
        //     var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();
        //
        //     // prepare query
        //     var query = $"{createdId}";
        //
        //     //send query
        //     var queryResponse = await fixture.Get(query, 10,
        //         check: response => new(response.StatusCode == HttpStatusCode.Created));
        //
        //     queryResponse.EnsureSuccessStatusCode();
        //
        //     var cartDetails = await queryResponse.GetResultFromJson<CartDetails>();
        //     cartDetails.Id.Should().Be(createdId);
        //     cartDetails.Status.Should().Be(CartStatus.Pending);
        //     cartDetails.ClientId.Should().Be(fixture.ClientId);
        //     cartDetails.Version.Should().Be(1);
        //     cartDetails.ProductItems.Should().BeEmpty();
        //     cartDetails.TotalPrice.Should().Be(0);
        // }
    }
}
