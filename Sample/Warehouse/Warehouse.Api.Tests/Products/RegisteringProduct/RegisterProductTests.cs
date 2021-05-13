using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Warehouse.Products.RegisteringProduct;
using Xunit;

namespace Warehouse.Api.Tests.Products.RegisteringProduct
{
    public class RegisteringProduct
    {
        public class RegisterProductFixture: ApiFixture
        {
            protected override string ApiUrl => "/api/products";

            protected override Func<IWebHostBuilder, IWebHostBuilder> SetupWebHostBuilder =>
                WarehouseTestWebHostBuilder.Configure;
        }

        public class RegisterProductTests: IClassFixture<RegisterProductFixture>
        {
            private readonly RegisterProductFixture fixture;

            public RegisterProductTests(RegisterProductFixture fixture)
            {
                this.fixture = fixture;
            }

            [Fact]
            public async Task ValidRequest_ShouldReturn_OK()
            {
                // Given
                const string sku = "test";
                const string name = "test";
                const string description = "test";
                var request = new RegisterProductRequest(sku, name, description);

                // When
                var response = await fixture.Post(request);

                // Then
                response.EnsureSuccessStatusCode();
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }
        }
    }
}
