using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Core.Api.Testing
{
    public class TestContext<TStartup>: TestContext
        where TStartup : class
    {
        public TestContext(
            Func<string, Dictionary<string, string>>? getConfiguration = null,
            Action<IServiceCollection>? setupServices = null,
            Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null,
            ITestOutputHelper? testOutputHelper = null
        ): base(
            getConfiguration,
            setupServices,
            (webHostBuilder =>
            {
                SetupWebHostBuilder(webHostBuilder);
                setupWebHostBuilder?.Invoke(webHostBuilder);
                return webHostBuilder;
            }),
            testOutputHelper)
        {
        }

        private static IWebHostBuilder SetupWebHostBuilder(IWebHostBuilder webHostBuilder)
            => webHostBuilder.UseStartup<TStartup>();
    }

    public class TestContext: IDisposable
    {
        public HttpClient Client { get; }

        public readonly TestServer Server;

        private readonly string fixtureName;

        public TestContext(
            Func<string, Dictionary<string, string>>? getConfiguration = null,
            Action<IServiceCollection>? setupServices = null,
            Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null,
            ITestOutputHelper? testOutputHelper = null
        )
        {
            fixtureName = new StackTrace().GetFrame(3)!.GetMethod()!.DeclaringType!.Name;

            Server = CreateTestServer(
                setupServices,
                setupWebHostBuilder,
                getConfiguration ?? (_ => new Dictionary<string, string>()),
                testOutputHelper
            );

            Client = Server.CreateClient();
        }

        private TestServer CreateTestServer(
            Action<IServiceCollection>? setupServices,
            Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder,
            Func<string, Dictionary<string, string>> getConfiguration,
            ITestOutputHelper? testOutputHelper)
        {
            setupWebHostBuilder ??= webHostBuilder => webHostBuilder;
            var configuration = getConfiguration(fixtureName);
            return new TestServer(
                setupWebHostBuilder
                (
                    TestWebHostBuilder.Create(
                        configuration,
                        services =>
                        {
                            ConfigureTestServices(services);
                            setupServices?.Invoke(services);
                        },
                        testOutputHelper)
                )
            );
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
