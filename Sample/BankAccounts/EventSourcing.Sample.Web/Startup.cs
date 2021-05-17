using System.Collections.Generic;
using Core;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.Events;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Clients.Domain.Clients.Handlers;
using EventSourcing.Sample.Clients.Storage;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Events;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Commands;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Events;
using EventSourcing.Sample.Transactions.Domain.Accounts;
using EventSourcing.Sample.Transactions.Domain.Accounts.Handlers;
using EventSourcing.Sample.Transactions.Domain.Clients.Handlers;
using EventSourcing.Sample.Transactions.Views.Accounts.AccountSummary;
using EventSourcing.Sample.Transactions.Views.Accounts.AllAccountsSummary;
using EventSourcing.Sample.Transactions.Views.Accounts.Handlers;
using EventSourcing.Sample.Transactions.Views.Clients;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Weasel.Postgresql;

namespace EventSourcing.Web.Sample
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc().AddNewtonsoftJson();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Sourcing Example", Version = "v1" });
            });

            services.AddTransient<IAccountNumberGenerator, RandomAccountNumberGenerator>();

            services.AddCoreServices();

            ConfigureMarten(services);

            ConfigureEf(services);

            ConfigureCqrs(services);
        }

        private static void ConfigureCqrs(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<CreateNewAccount, Unit>, AccountCommandHandler>();
            services.AddScoped<IRequestHandler<MakeTransfer, Unit>, AccountCommandHandler>();
            services.AddScoped<IRequestHandler<GetAccounts, IEnumerable<AccountSummary>>, GetAccountsHandler>();
            services.AddScoped<IRequestHandler<GetAccount, AccountSummary?>, GetAccountHandler>();
            services.AddScoped<IRequestHandler<GetAccountsSummary, AllAccountsSummary?>, GetAccountsSummaryHandler>();

            services.AddScoped<INotificationHandler<ClientCreated>, ClientsEventHandler>();
            services.AddScoped<INotificationHandler<ClientUpdated>, ClientsEventHandler>();
            services.AddScoped<INotificationHandler<ClientDeleted>, ClientsEventHandler>();

            services.AddScoped<IRequestHandler<CreateClient, Unit>, ClientsCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateClient, Unit>, ClientsCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteClient, Unit>, ClientsCommandHandler>();

            services.AddScoped<IRequestHandler<GetClients, List<ClientListItem>>, EventSourcing.Sample.Clients.Views.Clients.ClientsQueryHandler>();
            services.AddScoped<IRequestHandler<GetClient, ClientItem?>, EventSourcing.Sample.Clients.Views.Clients.ClientsQueryHandler>();
            services.AddScoped<IRequestHandler<GetClientView, ClientView?>, ClientsQueryHandler>();
        }

        private void ConfigureMarten(IServiceCollection services)
        {
            services.AddScoped(sp =>
            {
                var documentStore = DocumentStore.For(options =>
                {
                    var config = Configuration.GetSection("EventStore");
                    var connectionString = config.GetValue<string>("ConnectionString");
                    var schemaName = config.GetValue<string>("Schema");

                    options.Connection(connectionString);
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                    options.Events.DatabaseSchemaName = schemaName;
                    options.DatabaseSchemaName = schemaName;

                    options.Events.Projections.SelfAggregate<Account>();
                    options.Events.Projections.Add<AllAccountsSummaryViewProjection>();
                    options.Events.Projections.Add<AccountSummaryViewProjection>();
                    options.Events.Projections.Add<ClientsViewProjection>();

                    options.Events.AddEventType(typeof(NewAccountCreated));
                    options.Events.AddEventType(typeof(NewInflowRecorded));
                    options.Events.AddEventType(typeof(NewOutflowRecorded));

                    options.Events.AddEventType(typeof(ClientCreated));
                    options.Events.AddEventType(typeof(ClientUpdated));
                    options.Events.AddEventType(typeof(ClientDeleted));
                });

                return documentStore.OpenSession();
            });
        }

        private void ConfigureEf(IServiceCollection services)
        {
            services.AddDbContext<ClientsDbContext>(
                options => options.UseNpgsql(Configuration.GetConnectionString("ClientsDatabase")));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Sourcing Example V1");
            });

            app.ApplicationServices.GetRequiredService<ClientsDbContext>().Database.Migrate();
        }
    }
}
