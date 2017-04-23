using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using MediatR;
using System.Reflection;
using Domain.Commands;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using EventSourcing.Sample.Tasks.Domain.Accounts.Handlers;
using Marten;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Events;
using EventSourcing.Sample.Tasks.Contracts.Transactions.Events;
using EventSourcing.Sample.Tasks.Contracts.Accounts;
using EventSourcing.Sample.Tasks.Views.Accounts;
using EventSourcing.Sample.Tasks.Views.Accounts.Handlers;
using EventSourcing.Sample.Tasks.Domain.Accounts;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;

namespace EventSourcing.Web.Sample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
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
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Event Sourcing Example", Version = "v1" });
            });

            ConfigureMediator(services);

            ConfigureMarten(services);

            ConfigureCQRS(services);
        }

        private static void ConfigureCQRS(IServiceCollection services)
        {
            services.AddTransient<ICommandBus, CommandBus>();
            services.AddTransient<IQueryBus, QueryBus>();

            services.AddTransient<IRequestHandler<CreateNewAccount>, CreateNewAccountHandler>();
            services.AddTransient<IRequestHandler<MakeTransfer>, ProcessInflowHandler>();
            services.AddTransient<IRequestHandler<GetAccounts, IEnumerable<AccountSummary>>, GetAccountsHandler>();
        }

        private void ConfigureMarten(IServiceCollection services)
        {
            services.AddTransient(sp =>
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

                    options.Events.InlineProjections.AggregateStreamsWith<Account>();
                    options.Events.InlineProjections.AggregateStreamsWith<AccountsSummaryView>();
                    options.Events.InlineProjections.Add(new AccountSummaryViewProjection());
                });

                return documentStore.OpenSession();
            });
        }

        private static void ConfigureMediator(IServiceCollection services)
        {
            services.AddScoped<IMediator, Mediator>();
            services.AddTransient<SingleInstanceFactory>(sp => t => sp.GetService(t));
            services.AddTransient<MultiInstanceFactory>(sp => t => sp.GetServices(t));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Sourcing Example V1");
            });
        }
    }
}
