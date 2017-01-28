using Domain.Tests;

namespace Marten.Integration.Tests.TestsInfrasructure
{
    public static class DocumentStoreProvider
    {
        public static IDocumentStore Get(string schemaName = null)
        {
            return DocumentStore.For(_ =>
            {
                _.Connection(Settings.ConnectionString);
                _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Events.DatabaseSchemaName = schemaName ?? Settings.SchemaName;
            });
        }
    }
}
