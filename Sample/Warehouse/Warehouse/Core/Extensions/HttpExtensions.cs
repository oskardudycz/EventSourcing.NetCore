using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Warehouse.Core.Extensions
{
    public static class HttpExtensions
    {
        public static T FromRoute<T>(this HttpContext context, string name)
        {
            var value = context.Request.RouteValues[name];

            if (value is not T typedValue)
                throw new ArgumentOutOfRangeException(name);

            return typedValue;
        }

        public static T FromQuery<T>(this HttpContext context, string name)
        {
            var value = context.Request.Query[name];

            if (value is not T typedValue)
                throw new ArgumentOutOfRangeException(name);

            return typedValue;
        }

        public static async Task<T> FromBody<T>(this HttpContext context)
        {
            return await context.Request.ReadFromJsonAsync<T>() ??
                throw new ArgumentNullException("request");
        }

        public static Task OK<T>(this HttpContext context, T result)
            => context.ReturnJSON(result);

        public static Task Created<T>(this HttpContext context, T result)
            => context.ReturnJSON(result, HttpStatusCode.Created);

        public static async Task ReturnJSON<T>(this HttpContext context, T result, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            context.Response.StatusCode = (int)statusCode;

            if (result != null)
                return;

            await context.Response.WriteAsJsonAsync(result);
        }
    }
}
