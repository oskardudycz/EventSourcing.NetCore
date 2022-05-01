using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.Testing;

public class TestContext<TProgram>: IDisposable where TProgram : class
{
    public HttpClient Client { get; }
    public IServiceProvider Services => applicationFactory.Services;

    private readonly WebApplicationFactory<TProgram> applicationFactory;

    public TestContext()
    {
        applicationFactory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(
                builder =>
                {
                    var projectDir = Directory.GetCurrentDirectory();

                    builder
                        .UseEnvironment("Development")
                        .UseContentRoot(projectDir)
                        .UseConfiguration(new ConfigurationBuilder()
                            .SetBasePath(projectDir)
                            .AddJsonFile("appsettings.json", true)
                            .Build()
                        );
                }
            );


        Client = applicationFactory.CreateClient();
    }

    public void Dispose()
    {
        applicationFactory.Dispose();
        Client.Dispose();
    }
}
