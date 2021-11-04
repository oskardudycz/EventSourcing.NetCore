using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Warehouse.Core.Commands;

namespace Warehouse.Core.Extensions
{
internal static class EndpointsExtensions
{
    internal static IEndpointRouteBuilder MapCommand<TRequest>(
        this IEndpointRouteBuilder endpoints,
        HttpMethod httpMethod,
        string url,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        endpoints.MapMethods(url, new []{httpMethod.ToString()} , async context =>
        {
            var command = await context.FromBody<TRequest>();

            var commandResult = await context.SendCommand(command);

            if (commandResult == CommandResult.None)
            {
                context.Response.StatusCode = (int)statusCode;
                return;
            }

            await context.ReturnJSON(commandResult.Result, statusCode);
        });

        return endpoints;
    }
}
}
