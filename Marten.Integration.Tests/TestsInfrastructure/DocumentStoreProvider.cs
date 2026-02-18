using JasperFx;

namespace Marten.Integration.Tests.TestsInfrastructure;

public static class DocumentStoreProvider
{
    private const string SchemaName = "EventStore";

    public static IDocumentStore Get(string connectionString, string? schemaName = null, Action<StoreOptions>? setOptions = null) =>
        DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = schemaName ?? SchemaName;
            options.Events.DatabaseSchemaName = schemaName ?? SchemaName;

            setOptions?.Invoke(options);
        });
}
