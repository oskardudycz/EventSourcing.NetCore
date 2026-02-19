using System.Collections.Concurrent;
using System.Data;
using Marten.Integration.Tests.TestsInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marten.Integration.Tests.Tenancy;

public interface ITenancyPerSchemaStoreFactory
{
    IDocumentStore Get(string? tenantId);
}

public class TenancyPerSchemaStoreFactory(Action<string, StoreOptions> configure)
    : IDisposable, ITenancyPerSchemaStoreFactory
{
    private readonly ConcurrentDictionary<string, DocumentStore> stores = new ();

    public IDocumentStore Get(string? tenant) =>
        stores.GetOrAdd(tenant ?? "NO-TENANT", tenantId =>
        {
            var storeOptions = new StoreOptions();
            configure.Invoke(tenantId, storeOptions);
            return new DocumentStore(storeOptions);
        });

    public void Dispose()
    {
        foreach (var documentStore in stores.Values)
        {
            documentStore.Dispose();
        }
    }
}

public class DummyTenancyContext
{
    // this should be normally taken from the http context or other
    public string? TenantId { get; set; }
}

public class TenancyPerSchemaSessionFactory(
    ITenancyPerSchemaStoreFactory storeFactory,
    DummyTenancyContext tenancyContext)
    : ISessionFactory
{
    public IQuerySession QuerySession() =>
        storeFactory.Get(tenancyContext.TenantId).QuerySession();

    public IDocumentSession OpenSession() =>
        storeFactory.Get(tenancyContext.TenantId).LightweightSession(IsolationLevel.Serializable);
}

public record TestDocumentForTenancy(
    Guid Id,
    string Name
);

public class TenancyPerSchema(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer, false)
{
    private const string FirstTenant = "Tenant1";
    private const string SecondTenant = "Tenant2";

    [Fact]
    public async Task GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
    {
        var services = new ServiceCollection();

        AddMarten(services);

        await using var sp = services.BuildServiceProvider();
        // simulate scope per HTTP request with different tenant
        using (var firstScope = sp.CreateScope())
        {
            firstScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().TenantId = FirstTenant;

            await using (var session = firstScope.ServiceProvider.GetRequiredService<IDocumentSession>())
            {
                session.Insert(new TestDocumentForTenancy(Guid.CreateVersion7(), FirstTenant));
                await session.SaveChangesAsync();
            }
        }

        // simulate scope per HTTP request with different tenant
        using (var secondScope = sp.CreateScope())
        {
            secondScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().TenantId = SecondTenant;

            await using (var session = secondScope.ServiceProvider.GetRequiredService<IDocumentSession>())
            {
                session.Insert(new TestDocumentForTenancy(Guid.CreateVersion7(), SecondTenant));
                await session.SaveChangesAsync();
            }
        }
    }

    private void AddMarten(IServiceCollection services)
    {
        // simulate http context
        services.AddScoped<DummyTenancyContext>();

        services.AddSingleton<ITenancyPerSchemaStoreFactory, TenancyPerSchemaStoreFactory>();

        // register options as function to resolve it per tenant
        services.AddSingleton<Action<string, StoreOptions>>((tenantId, options) =>
        {
            options.DatabaseSchemaName = tenantId;
            options.Connection(ConnectionString);
        });

        services.AddScoped<ISessionFactory, TenancyPerSchemaSessionFactory>();

        services.AddScoped(s => s.GetRequiredService<ISessionFactory>().QuerySession());
        services.AddScoped(s => s.GetRequiredService<ISessionFactory>().OpenSession());
    }
}
