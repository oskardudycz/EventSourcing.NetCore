using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace Core.Api.Testing
{
    public static class ResponseExtensions
    {
        public static async Task<T> GetResultFromJson<T>(this HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();

            result.Should().NotBeNull()
                .And.Should().NotBe(string.Empty);

            return result.FromJson<T>();
        }
    }
}
