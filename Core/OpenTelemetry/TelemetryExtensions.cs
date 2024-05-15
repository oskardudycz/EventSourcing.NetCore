using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;

namespace Core.OpenTelemetry;

public static class TelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        string serviceName
    ) => AddOpenTelemetry(services, serviceName, OpenTelemetryOptions.Default);


    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder,
        string serviceName,
        OpenTelemetryOptions? options = null
    )
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry(serviceName, options ?? OpenTelemetryOptions.Default);

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        OpenTelemetryOptions options
    )
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
                options.ConfigureMeterProvider(
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        // .SetResourceBuilder(
                        //     ResourceBuilder.CreateDefault()
                        //         .AddService(serviceName)
                        //         .AddTelemetrySdk()
                        // )
                )
            )
            .WithTracing(tracing =>
                {
                    options.ConfigureTracerProvider(
                        tracing.AddAspNetCoreInstrumentation()
                            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                            //.AddGrpcClientInstrumentation()
                            .AddHttpClientInstrumentation()
                    );

                    if (!options.ShouldDisableConsoleExporter)
                        tracing.AddConsoleExporter();
                }
            );

        return services;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Prometheus exporter (requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
        // builder.Services.AddOpenTelemetry()
        //    .WithMetrics(metrics => metrics.AddPrometheusExporter());

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
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
    public Func<MeterProviderBuilder, MeterProviderBuilder> ConfigureMeterProvider { get; private set; } = p => p;

    public bool ShouldDisableConsoleExporter { get; private set; }

    private OpenTelemetryOptions()
    {
    }

    public static OpenTelemetryOptions Build(Func<OpenTelemetryOptions, OpenTelemetryOptions> build) =>
        build(Default);

    public OpenTelemetryOptions WithTracing(Func<TracerProviderBuilder, TracerProviderBuilder> configure)
    {
        this.ConfigureTracerProvider = configure;
        return this;
    }

    public OpenTelemetryOptions WithMetrics(Func<MeterProviderBuilder, MeterProviderBuilder> configure)
    {
        this.ConfigureMeterProvider = configure;
        return this;
    }

    public OpenTelemetryOptions DisableConsoleExporter(bool shouldDisable)
    {
        ShouldDisableConsoleExporter = shouldDisable;
        return this;
    }
}
