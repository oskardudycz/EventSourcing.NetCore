namespace Marten.Integration.Tests.TestsInfrasructure
{
    public static class DocumentSessionProvider
    {
        public static IDocumentSession Get(string schemaName = null)
        {
            var store = DocumentStoreProvider.Get(schemaName);

            return store.OpenSession();
        }
    }
}
