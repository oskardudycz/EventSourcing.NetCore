using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Api.Testing
{
    public class TestContext<TStartup>: TestContext
        where TStartup : class
    {
        public TestContext(
            Func<string, Dictionary<string, string>>? getConfiguration = null,
            Action<IServiceCollection>? setupServices = null,
            Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null
        ): base(getConfiguration, setupServices, (webHostBuilder =>
        {
            SetupWebHostBuilder(webHostBuilder);
            setupWebHostBuilder?.Invoke(webHostBuilder);
            return webHostBuilder;
        }))
        {
        }

        private static IWebHostBuilder SetupWebHostBuilder(IWebHostBuilder webHostBuilder)
            => webHostBuilder.UseStartup<TStartup>();
    }

    public class TestContext: IDisposable
    {
        public HttpClient Client { get; }

        public readonly TestServer Server;

        private readonly Func<string, Dictionary<string, string>> getConfiguration =
            _ => new Dictionary<string, string>();

        public TestContext(
            Func<string, Dictionary<string, string>>? getConfiguration = null,
            Action<IServiceCollection>? setupServices = null,
            Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null
        )
        {
            if (getConfiguration != null)
            {
                this.getConfiguration = getConfiguration;
            }

            var fixtureName = new StackTrace().GetFrame(3)!.GetMethod()!.DeclaringType!.Name;

            var configuration = this.getConfiguration(fixtureName);

            setupWebHostBuilder ??= webHostBuilder => webHostBuilder;
            Server = new TestServer(setupWebHostBuilder(TestWebHostBuilder.Create(configuration, services =>
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
}
