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
using Microsoft.Net.Http.Headers;

namespace Core.WebApi.OptimisticConcurrency;

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

        // It's needed to do it in the event handler,
        // as headers cannot be modified after the header was
        context.Response.OnStarting(() =>
        {
            TrySetETagResponseHeader(context, nextResourceVersionProvider);
            return Task.CompletedTask;
        });

        await next(context);
    }

    private void TryGetExpectedVersionFromRequestIfMatchHeader(
        HttpContext context,
        IExpectedResourceVersionProvider expectedResourceVersionProvider
    )
    {
        if (!SupportedMethods.Contains(context.Request.Method)) return;

        var ifMatchHeader = context.GetIfMatchRequestHeader();

        if (ifMatchHeader == null || Equals(ifMatchHeader, EntityTagHeaderValue.Any)) return;

        if (!expectedResourceVersionProvider.TrySet(ifMatchHeader.GetSanitizedValue()))
            throw new ArgumentOutOfRangeException(nameof(ifMatchHeader), "Invalid format of If-Match header value");
    }

    private static void TrySetETagResponseHeader(
        HttpContext context,
        INextResourceVersionProvider nextResourceVersionProvider
    )
    {
        var nextExpectedVersion = nextResourceVersionProvider.Value;
        if (nextExpectedVersion == null) return;

        context.TrySetETagResponseHeader(nextExpectedVersion);
    }
}

public static class OptimisticConcurrencyMiddlewareConfig
{
    public static IServiceCollection AddOptimisticConcurrencyMiddleware(
        this IServiceCollection services,
        Func<IServiceProvider, Func<string, bool>>? trySetExpectedVersion = null,
        Func<IServiceProvider, Func<string?>>? getNextVersion = null
    )
    {
        services.TryAddScoped<IExpectedResourceVersionProvider>(sp =>
            new ExpectedResourceVersionProvider(trySetExpectedVersion?.Invoke(sp)));
        services.TryAddScoped<INextResourceVersionProvider>(sp =>
            new NextResourceVersionProvider(getNextVersion?.Invoke(sp)));

        return services;
    }

    public static IApplicationBuilder UseOptimisticConcurrencyMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<OptimisticConcurrencyMiddleware>();
}
