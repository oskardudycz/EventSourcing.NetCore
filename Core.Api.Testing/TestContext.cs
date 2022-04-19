using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.Testing;

public class TestContext<TStartup>: TestContext
    where TStartup : class
{
    public TestContext(
        Action<IServiceCollection>? setupServices = null,
        Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null
    ): base(setupServices, (webHostBuilder =>
    {
        SetupWebHostBuilder(webHostBuilder);
        setupWebHostBuilder?.Invoke(webHostBuilder);
        return webHostBuilder;
    }))
    {
    }

    private static IWebHostBuilder SetupWebHostBuilder(IWebHostBuilder webHostBuilder) =>
        webHostBuilder.UseStartup<TStartup>();
}

public class TestContext: IDisposable
{
    public HttpClient Client { get; }

    public readonly TestServer Server;
    public TestContext(
        Action<IServiceCollection>? setupServices = null,
        Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null
    )
    {
        setupWebHostBuilder ??= webHostBuilder => webHostBuilder;
        Server = new TestServer(setupWebHostBuilder(TestWebHostBuilder.Create(services =>
        {
            ConfigureTestServices(services);
            setupServices?.Invoke(services);
        })));


        Client = Server.CreateClient();
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
    }

    public void Dispose()
    {
        Server.Dispose();
        Client.Dispose();
    }
}
