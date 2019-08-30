using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Configuration
{
    public class MartenConfig
    {
    }

    internal static class MartenConfigExtensions
    {
        internal static void AddMarten(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IDocumentStore>(sp => DocumentStore.For(options => SetStoreOptions(options, config)));
            services.AddSingleton(sp => sp.GetRequiredService<IDocumentStore>().OpenSession());
        }

        private static void SetStoreOptions(StoreOptions options, IConfiguration config)
        {
            var martenConfig = config.GetSection("EventStore");
            var connectionString = martenConfig.GetSection("ConnectionString").Value;
            var schemaName = martenConfig.GetSection("Schema").Value;

            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Events.DatabaseSchemaName = schemaName;
            options.DatabaseSchemaName = schemaName;
        }
    }
}
