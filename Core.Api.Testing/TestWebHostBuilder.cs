using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Core.Api.Testing
{
    public static class TestWebHostBuilder
    {
        public static IWebHostBuilder Create(
            Dictionary<string, string> configuration,
            Action<IServiceCollection>? configureServices = null,
            IMessageSink? messageSink = null)
        {
            var projectDir = Directory.GetCurrentDirectory();
            configureServices ??= _ => { };

            return new WebHostBuilder()
                .UseEnvironment("Development")
                .UseContentRoot(projectDir)
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(projectDir)
                    .AddJsonFile("appsettings.json", true)
                    .AddInMemoryCollection(configuration)
                    .Build()
                )
                .ConfigureServices(configureServices)
                .ConfigureLogging(loggingBuilder =>
                {
                    if (messageSink != null)
                    {
                        loggingBuilder.Services.AddSingleton<ILoggerProvider>(_ =>
                            new XUnitLoggerProvider(testOutputHelper));
                    }
                });
        }
    }
}
