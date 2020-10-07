using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;

namespace Shipments.Api.Tests.Core
{
    public static class ResponseExtensions
    {
        public static async Task<T> GetResultFromJSON<T>(this HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();

            return result.FromJson<T>();
        }
    }
}
