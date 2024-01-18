namespace Marten.Integration.Tests.TestsInfrastructure;

public static class DocumentSessionProvider
{
    public static IDocumentSession Get(string connectionString, string? schemaName = null, Action<StoreOptions>? setOptions = null)
    {
        var store = DocumentStoreProvider.Get(connectionString, schemaName, setOptions);

        return store.LightweightSession();
    }
}
