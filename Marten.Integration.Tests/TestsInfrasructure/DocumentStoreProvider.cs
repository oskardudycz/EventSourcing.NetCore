using Domain.Tests;

namespace Marten.Integration.Tests.TestsInfrasructure
{
    public static class DocumentStoreProvider
    {
        public static IDocumentStore Get(string schemaName = null)
        {
            return DocumentStore.For(options =>
            {
                options.Connection(Settings.ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.DatabaseSchemaName = schemaName ?? Settings.SchemaName;
            });
        }
    }
}
