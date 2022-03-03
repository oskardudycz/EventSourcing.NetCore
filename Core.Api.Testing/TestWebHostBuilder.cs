using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.Testing;

public static class TestWebHostBuilder
{
    public static IWebHostBuilder Create(Dictionary<string, string> configuration, Action<IServiceCollection>? configureServices = null)
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
            .ConfigureServices(configureServices);
    }
}