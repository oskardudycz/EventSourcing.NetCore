using System;

namespace Marten.Integration.Tests.TestsInfrasructure
{
    public static class DocumentStoreProvider
    {
        public static IDocumentStore Get(string schemaName = null, Action<StoreOptions> setOptions = null)
        {
            return DocumentStore.For(options =>
            {
                options.Connection(Settings.ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.DatabaseSchemaName = schemaName ?? Settings.SchemaName;
                options.Events.DatabaseSchemaName = schemaName ?? Settings.SchemaName;

                if (setOptions != null)
                    setOptions(options);
            });
        }
    }
}