using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marten.Integration.Tests.Tenancy;

public interface ITenancyPerSchemaStoreFactory
{
    IDocumentStore Get(string? tenantId);
}

public class TenancyPerSchemaStoreFactory : IDisposable, ITenancyPerSchemaStoreFactory
{
    private readonly Action<string, StoreOptions> configure;
    private readonly ConcurrentDictionary<string, DocumentStore> stores = new ();

    public TenancyPerSchemaStoreFactory(Action<string, StoreOptions> configure)
    {
        this.configure = configure;
    }

    public IDocumentStore Get(string? tenant)
    {
        return stores.GetOrAdd(tenant ?? "NO-TENANT", tenantId =>
        {
            var storeOptions = new StoreOptions();
            configure.Invoke(tenantId, storeOptions);
            return new DocumentStore(storeOptions);
        });
    }

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

public class TenancyPerSchemaSessionFactory: ISessionFactory
{
    private readonly ITenancyPerSchemaStoreFactory storeFactory;
    private readonly DummyTenancyContext tenancyContext;

    public TenancyPerSchemaSessionFactory(ITenancyPerSchemaStoreFactory storeFactory, DummyTenancyContext tenancyContext)
    {
        this.storeFactory = storeFactory;
        this.tenancyContext = tenancyContext;
    }

    public IQuerySession QuerySession()
    {
        return storeFactory.Get(tenancyContext.TenantId).QuerySession();
    }

    public IDocumentSession OpenSession()
    {
        return storeFactory.Get(tenancyContext.TenantId).LightweightSession(IsolationLevel.Serializable);
    }
}

public record TestDocumentForTenancy(
    Guid Id,
    string Name
);

public class TenancyPerSchema
{
    private const string FirstTenant = "Tenant1";
    private const string SecondTenant = "Tenant2";

    [Fact]
    public void GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
    {
        var services = new ServiceCollection();

        AddMarten(services);

        using (var sp = services.BuildServiceProvider())
        {
            // simulate scope per HTTP request with different tenant
            using (var firstScope = sp.CreateScope())
            {
                firstScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().TenantId = FirstTenant;

                using (var session = firstScope.ServiceProvider.GetRequiredService<IDocumentSession>())
                {
                    session.Insert(new TestDocumentForTenancy(Guid.NewGuid(), FirstTenant));
                    session.SaveChanges();
                }
            }

            // simulate scope per HTTP request with different tenant
            using (var secondScope = sp.CreateScope())
            {
                secondScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().TenantId = SecondTenant;

                using (var session = secondScope.ServiceProvider.GetRequiredService<IDocumentSession>())
                {
                    session.Insert(new TestDocumentForTenancy(Guid.NewGuid(), SecondTenant));
                    session.SaveChanges();
                }
            }
        }
    }

    private static void AddMarten(IServiceCollection services)
    {
        // simulate http context
        services.AddScoped<DummyTenancyContext>();

        services.AddSingleton<ITenancyPerSchemaStoreFactory, TenancyPerSchemaStoreFactory>();

        // register options as function to resolve it per tenant
        services.AddSingleton<Action<string, StoreOptions>>((tenantId, options) =>
        {
            options.DatabaseSchemaName = tenantId;
            options.Connection(Settings.ConnectionString);
        });

        services.AddScoped<ISessionFactory, TenancyPerSchemaSessionFactory>();

        services.AddScoped(s => s.GetRequiredService<ISessionFactory>().QuerySession());
        services.AddScoped(s => s.GetRequiredService<ISessionFactory>().OpenSession());
    }
}