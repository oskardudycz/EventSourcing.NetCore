using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Commands;
using Core.Events;
using Core.Events.External;
using Core.Requests;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipments.Api.Tests.Core;

namespace Core.Testing
{
    public class TestContext<TStartup>: IDisposable
        where TStartup : class
    {
        public HttpClient Client { get; private set; }

        private TestServer server;

        private readonly EventsLog eventsLog = new EventsLog();
        private readonly DummyExternalEventProducer externalEventProducer = new DummyExternalEventProducer();
        private readonly DummyExternalCommandBus externalCommandBus = new DummyExternalCommandBus();

        private readonly Func<string, Dictionary<string, string>> getConfiguration = fixtureName => new Dictionary<string, string>();

        public TestContext(Func<string, Dictionary<string, string>> getConfiguration = null)
        {
            if (getConfiguration != null)
            {
                this.getConfiguration = getConfiguration;
            }
            SetUpClient();
        }

        private void SetUpClient()
        {
            var fixtureName = new StackTrace().GetFrame(3).GetMethod().DeclaringType.Name;

            var configuration = getConfiguration(fixtureName);
            var projectDir = Directory.GetCurrentDirectory();

            server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Tests")
                .UseContentRoot(projectDir)
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(projectDir)
                    .AddJsonFile("appsettings.json", true)
                    .AddInMemoryCollection(configuration)
                    .Build()
                )
                .ConfigureServices(services =>
                {
                    services.AddSingleton(eventsLog);
                    services.AddSingleton(typeof(INotificationHandler<>), typeof(EventListener<>));
                    services.AddSingleton<IExternalEventProducer>(externalEventProducer);
                    services.AddSingleton<IExternalCommandBus>(externalCommandBus);
                    services.AddSingleton<IExternalEventConsumer, DummyExternalEventConsumer>();
                })
                .UseStartup<TStartup>());

            Client = server.CreateClient();
        }

        public IReadOnlyCollection<TEvent> PublishedExternalEventsOfType<TEvent>() where TEvent: IExternalEvent
        {
            return externalEventProducer.PublishedEvents.OfType<TEvent>().ToList();
        }

        public IReadOnlyCollection<TCommand> PublishedExternalCommandOfType<TCommand>() where TCommand: ICommand
        {
            return externalCommandBus.SentCommands.OfType<TCommand>().ToList();
        }

        public async Task PublishInternalEvent(IEvent @event)
        {
            using (var scope = server.Host.Services.CreateScope())
            {
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                await eventBus.Publish(@event);
            }
        }

        public void Dispose()
        {
            server?.Dispose();
            Client?.Dispose();
        }

        public IReadOnlyCollection<TEvent> PublishedInternalEventsOfType<TEvent>()
        {
            return eventsLog.PublishedEvents.OfType<TEvent>().ToList();
        }
    }
}
