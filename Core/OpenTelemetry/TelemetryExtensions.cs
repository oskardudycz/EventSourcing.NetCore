using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Core.OpenTelemetry;

public static class TelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        string serviceName
    ) => AddOpenTelemetry(services, serviceName, OpenTelemetryOptions.Default);

    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        OpenTelemetryOptions options
    )
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        services
            .AddSingleton<IActivityScope, ActivityScope>()
            .AddOpenTelemetryTracing(builder =>
            {
                options.ConfigureTracerProvider(builder
                        .AddSource(ActivitySourceProvider.DefaultSourceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation(o =>
                        {
                            o.RecordException = true;
                        })
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(serviceName)
                                .AddTelemetrySdk()
                        ))
                    .SetSampler(new AlwaysOnSampler());

                if (!options.ShouldDisableConsoleExporter)
                    builder.AddConsoleExporter();
            });

        return services;
    }
}

public class OpenTelemetryOptions
{
    public static OpenTelemetryOptions Default
    {
        get
        {
            return new OpenTelemetryOptions();
        }
    }

    public Func<TracerProviderBuilder, TracerProviderBuilder> ConfigureTracerProvider { get; private set; } = p => p;

    public bool ShouldDisableConsoleExporter { get; private set; }

    private OpenTelemetryOptions()
    {
    }

    public static OpenTelemetryOptions Build(Func<OpenTelemetryOptions, OpenTelemetryOptions> build) =>
        build(Default);

    public OpenTelemetryOptions Configure(Func<TracerProviderBuilder, TracerProviderBuilder> configure)
    {
        this.ConfigureTracerProvider = configure;
        return this;
    }

    public OpenTelemetryOptions DisableConsoleExporter(bool shouldDisable)
    {
        ShouldDisableConsoleExporter = shouldDisable;
        return this;
    }
}
