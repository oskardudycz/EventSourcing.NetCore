using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.OptimisticConcurrency;
using Core.WebApi.Headers;
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
        IExpectedResourceVersionProvider expectedResourceVersionProvider,
        INextResourceVersionProvider nextResourceVersionProvider
    )
    {
        TryGetExpectedVersionFromRequestIfMatchHeader(context, expectedResourceVersionProvider);

        await next(context);

        TrySetETagResponseHeader(context, nextResourceVersionProvider);
    }

    private void TryGetExpectedVersionFromRequestIfMatchHeader(
        HttpContext context,
        IExpectedResourceVersionProvider expectedResourceVersionProvider
    )
    {
        if (!SupportedMethods.Contains(context.Request.Method)) return;

        var ifMatchHeader = context.GetIfMatchRequestHeader();

        if (ifMatchHeader == null || Equals(ifMatchHeader, EntityTagHeaderValue.Any)) return;

        if (!expectedResourceVersionProvider.TrySet(ifMatchHeader.Tag.Value))
            throw new ArgumentOutOfRangeException(nameof(ifMatchHeader), "Invalid format of If-Match header value");
    }

    private static void TrySetETagResponseHeader(
        HttpContext context,
        INextResourceVersionProvider nextResourceVersionProvider
    )
    {
        var nextExpectedVersion = nextResourceVersionProvider.Value;
        if (nextExpectedVersion == null) return;

        context.Response.GetTypedHeaders().ETag = new EntityTagHeaderValue(nextExpectedVersion, true);
    }
}

public static class ConditionalRequestMiddlewareConfig
{
    public static IServiceCollection AddOptimisticConcurrencyMiddleware(this IServiceCollection services)
    {
        services.TryAddScoped<DefaultExpectedResourceVersionProvider, DefaultExpectedResourceVersionProvider>();
        services.TryAddScoped<DefaultNextResourceVersionProvider, DefaultNextResourceVersionProvider>();

        return services;
    }

    public static IApplicationBuilder UseOptimisticConcurrencyMiddleware(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService(typeof(DefaultExpectedResourceVersionProvider)) == null ||
            app.ApplicationServices.GetService(typeof(DefaultNextResourceVersionProvider)) == null)
        {
            throw new InvalidOperationException(
                "Unable to find the required services. You must call the appropriate AddOptimisticConcurrencyMiddleware method in ConfigureServices in the application startup code.");
        }

        return app.UseMiddleware<OptimisticConcurrencyMiddleware>();
    }
}
