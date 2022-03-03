namespace Marten.Integration.Tests.TestsInfrastructure;

public static class DocumentSessionProvider
{
    public static IDocumentSession Get(string? schemaName = null, Action<StoreOptions>? setOptions = null)
    {
        var store = DocumentStoreProvider.Get(schemaName, setOptions);

        return store.OpenSession();
    }
}