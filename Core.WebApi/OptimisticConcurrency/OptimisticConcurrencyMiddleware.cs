using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.WebApi.Tracing.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Core.WebApi.OptimisticConcurrency;

// Inspired by Tomasz Pęczek article: https://www.tpeczek.com/2017/11/handling-conditional-requests-in-aspnet.html

public class OptimisticConcurrencyMiddleware
{
    private readonly RequestDelegate next;

    private readonly string[] SupportedMethods =
    {
        HttpMethod.Post.Method, HttpMethod.Put.Method, HttpMethod.Delete.Method
    };

    public OptimisticConcurrencyMiddleware(
        RequestDelegate next
    )
    {
        this.next = next;
    }

    public async Task Invoke(
        HttpContext context,
        ExpectedResourceVersionProvider expectedResourceVersionProvider,
        NextResourceVersionProvider nextResourceVersionProvider
    )
    {
        TryGetExpectedVersionFromRequestIfMatchHeader(context, expectedResourceVersionProvider);

        await next(context);

        TrySetETagResponseHeader(context, nextResourceVersionProvider);
    }

    private void TryGetExpectedVersionFromRequestIfMatchHeader(
        HttpContext context,
        ExpectedResourceVersionProvider expectedResourceVersionProvider
    )
    {
        if (!SupportedMethods.Contains(context.Request.Method)) return;

        var ifMatchHeader = GetIfMatchHeader(context);

        if (ifMatchHeader != null && !Equals(ifMatchHeader, EntityTagHeaderValue.Any))
        {
            expectedResourceVersionProvider.Set(new ResourceVersion(ifMatchHeader.ToString()));
        }
    }

    private static EntityTagHeaderValue? GetIfMatchHeader(HttpContext context)
    {
        return context.Request.GetTypedHeaders().IfMatch.FirstOrDefault();
    }

    private static void TrySetETagResponseHeader(HttpContext context,
        NextResourceVersionProvider nextResourceVersionProvider)
    {
        if (!IsSuccessStatusCode(context.Response.StatusCode))
            return;

        var nextExpectedVersion = nextResourceVersionProvider.Get();
        if (nextExpectedVersion == null)
            return;

        context.Response.GetTypedHeaders().ETag =
            new EntityTagHeaderValue(new StringSegment(nextExpectedVersion.Value), true);
    }

    private static bool IsSuccessStatusCode(int statusCode) =>
        statusCode is >= 200 and <= 299;
}

public static class ConditionalRequestMiddlewareConfig
{
    public static IServiceCollection AddOptimisticConcurrencyMiddleware(this IServiceCollection services)
    {
        services.TryAddScoped<ExpectedResourceVersionProvider, ExpectedResourceVersionProvider>();
        services.TryAddScoped<NextResourceVersionProvider, NextResourceVersionProvider>();

        return services;
    }

    public static IApplicationBuilder UseOptimisticConcurrencyMiddleware(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService(typeof(ExpectedResourceVersionProvider)) == null ||
            app.ApplicationServices.GetService(typeof(NextResourceVersionProvider)) == null)
        {
            throw new InvalidOperationException(
                "Unable to find the required services. You must call the appropriate AddOptimisticConcurrencyMiddleware method in ConfigureServices in the application startup code.");
        }

        return app.UseMiddleware<OptimisticConcurrencyMiddleware>();
    }
}
