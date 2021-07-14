using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Extensions.DependencyInjection;
using Shipments.Api.Tests.Core;

namespace Core.Testing
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

        private readonly TestServer server;

        private readonly EventsLog eventsLog = new();
        private readonly DummyExternalEventProducer externalEventProducer = new();
        private readonly DummyExternalCommandBus externalCommandBus = new();

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
            server = new TestServer(setupWebHostBuilder(TestWebHostBuilder.Create(configuration, services =>
            {
                ConfigureTestServices(services);
                setupServices?.Invoke(services);
            })));


            Client = server.CreateClient();
        }

        protected void ConfigureTestServices(IServiceCollection services)
        {
            services.AddSingleton(eventsLog);
            services.AddSingleton(typeof(INotificationHandler<>), typeof(EventListener<>));
            services.AddSingleton<IExternalEventProducer>(externalEventProducer);
            services.AddSingleton<IExternalCommandBus>(externalCommandBus);
            services.AddSingleton<IExternalEventConsumer, DummyExternalEventConsumer>();
        }

        public IReadOnlyCollection<TEvent> PublishedExternalEventsOfType<TEvent>() where TEvent : IExternalEvent
        {
            return externalEventProducer.PublishedEvents.OfType<TEvent>().ToList();
        }

        public IReadOnlyCollection<TCommand> PublishedExternalCommandOfType<TCommand>() where TCommand : ICommand
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
            server.Dispose();
            Client.Dispose();
        }

        public IReadOnlyCollection<TEvent> PublishedInternalEventsOfType<TEvent>()
        {
            return eventsLog.PublishedEvents.OfType<TEvent>().ToList();
        }
    }
}
