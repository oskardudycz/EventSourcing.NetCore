using System.Net.Http;
using System.Threading.Tasks;
using Core.Serialization.Newtonsoft;
using FluentAssertions;

namespace Core.Api.Testing;

public static class ResponseExtensions
{
    public static async Task<T> GetResultFromJson<T>(this HttpResponseMessage response)
    {
        var result = await response.Content.ReadAsStringAsync();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        return result.FromJson<T>();
    }
}
