using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Events;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Api.Tests.Core;

namespace EventSourcing.Sample.IntegrationTests.Infrastructure
{
    public class TestContext<TStartup>: IDisposable
        where TStartup : class
    {
        public HttpClient Client { get; private set; }

        private TestServer server;

        private readonly EventsLog eventsLog = new EventsLog();

        public TestContext()
        {
            SetUpClient();
        }

        private void SetUpClient()
        {
            var fixtureName = new StackTrace().GetFrame(3).GetMethod().DeclaringType.Name;

            var configuration = new Dictionary<string, string>
            {
                {
                    "EventStore:ConnectionString",
                    "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'"
                },
                {"EventStore:WriteModelSchema", $"{fixtureName}Write"},
                {"EventStore:ReadModelSchema", $"{fixtureName}Read"},
                {"EventStore:ShouldRecreateDatabase", "true"}
            };

            var projectDir = Directory.GetCurrentDirectory();

            server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Tests")
                .UseContentRoot(projectDir)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddInMemoryCollection(configuration)
                    .Build()
                )
                .ConfigureServices(services =>
                {
                    services.AddSingleton(eventsLog);
                    services.AddSingleton(typeof(INotificationHandler<>), typeof(EventListener<>));
                })
                .UseStartup<TStartup>());

            Client = server.CreateClient();
        }

        // public async Task PublishInternalEvent(IEvent @event)
        // {
        //     using (var scope = server.Host.Services.CreateScope())
        //     {
        //         var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        //         await eventBus.Publish(@event);
        //     }
        // }

        public void Dispose()
        {
            server?.Dispose();
            Client?.Dispose();
        }

        public IReadOnlyCollection<TEvent> PublishedEventsOfType<TEvent>()
        {
            return eventsLog.PublishedEvents.OfType<TEvent>().ToList();
        }
    }
}
