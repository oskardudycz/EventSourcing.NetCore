// using System;
// using System.Net;
// using System.Threading.Tasks;
// using Core.Testing;
// using FluentAssertions;
// using Microsoft.AspNetCore.Hosting;
// using Warehouse.Products.GettingProductDetails;
// using Warehouse.Products.RegisteringProduct;
// using Xunit;
//
// namespace Warehouse.Api.Tests.Products.GettingProductDetails
// {
//     public class GetProductDetailsFixture: ApiFixture
//     {
//         protected override string ApiUrl => "/api/products";
//
//         protected override Func<IWebHostBuilder, IWebHostBuilder> SetupWebHostBuilder =>
//             whb => WarehouseTestWebHostBuilder.Configure(whb, nameof(GetProductDetailsFixture));
//
//         public ProductDetails ExistingProduct = default!;
//
//         public Guid ProductId = default!;
//
//         public override async Task InitializeAsync()
//         {
//             var registerProduct = new RegisterProductRequest("IN11111", "ValidName", "ValidDescription");
//             var registerResponse = await Post(registerProduct);
//
//             registerResponse.EnsureSuccessStatusCode()
//                 .StatusCode.Should().Be(HttpStatusCode.Created);
//
//             ProductId = await registerResponse.GetResultFromJson<Guid>();
//
//             var (sku, name, description) = registerProduct;
//             ExistingProduct = new ProductDetails(ProductId, sku!, name!, description);
//         }
//     }
//
//     public class GetProductDetailsTests: IClassFixture<GetProductDetailsFixture>
//     {
//         private readonly GetProductDetailsFixture fixture;
//
//         public GetProductDetailsTests(GetProductDetailsFixture fixture)
//         {
//             this.fixture = fixture;
//         }
//
//         [Fact]
//         public async Task ValidRequest_With_NoParams_ShouldReturn_200()
//         {
//             // Given
//
//             // When
//             var response = await fixture.Get(fixture.ProductId.ToString());
//
//             // Then
//             response.EnsureSuccessStatusCode()
//                 .StatusCode.Should().Be(HttpStatusCode.OK);
//
//             var product = await response.GetResultFromJson<ProductDetails>();
//             product.Should().NotBeNull();
//             product.Should().BeEquivalentTo(fixture.ExistingProduct);
//         }
//
//         [Theory]
//         [InlineData(12)]
//         [InlineData("not-a-guid")]
//         public async Task InvalidGuidId_ShouldReturn_400(object invalidId)
//         {
//             // Given
//
//             // When
//             var response = await fixture.Get($"{invalidId}");
//
//             // Then
//             response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//         }
//
//         [Fact]
//         public async Task NotExistingId_ShouldReturn_404()
//         {
//             // Given
//             var notExistingId = Guid.NewGuid();
//
//             // When
//             var response = await fixture.Get($"{notExistingId}");
//
//             // Then
//             response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//         }
//     }
// }
