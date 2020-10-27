using System;
using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marten.Integration.Tests.Tenancy
{
    public interface ITenancyPerSchemaStoreFactory
    {
        IDocumentStore Get(string tenantId);
    }

    public class TenancyPerSchemaStoreFactory : IDisposable, ITenancyPerSchemaStoreFactory
    {
        private readonly Action<string, StoreOptions> configure;
        private readonly ConcurrentDictionary<string, DocumentStore> stores = new ConcurrentDictionary<string, DocumentStore>();

        public TenancyPerSchemaStoreFactory(Action<string, StoreOptions> configure)
        {
            this.configure = configure;
        }

        public IDocumentStore Get(string tenant)
        {
            return stores.GetOrAdd(tenant, (tenantId) =>
            {
                var storeOptions = new StoreOptions();
                configure?.Invoke(tenantId, storeOptions);
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
        public string TenantId { get; set; }
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

    public class TestDocumentForTenancy
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class TenancyPerSchema
    {
        private const string FirstTenant = "Tenant1";
        private const string SecondTenant = "Tenant2";

        [Fact]
        public void GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
        {
            var services = new ServiceCollection();
            var tenancyContext = new DummyTenancyContext();

            AddMarten(services, tenancyContext);

            using (var sp = services.BuildServiceProvider())
            {
                // simulate scope per HTTP request with different tenant
                using (var firstScope = sp.CreateScope())
                {
                    tenancyContext.TenantId = "Tenant1";

                    using (var session = firstScope.ServiceProvider.GetRequiredService<IDocumentSession>())
                    {
                        session.Insert(new TestDocumentForTenancy{ Id = Guid.NewGuid(), Name = FirstTenant});
                        session.SaveChanges();
                    }
                }

                // simulate scope per HTTP request with different tenant
                using (var secondScope = sp.CreateScope())
                {
                    tenancyContext.TenantId = SecondTenant;

                    using (var session = secondScope.ServiceProvider.GetRequiredService<IDocumentSession>())
                    {
                        session.Insert(new TestDocumentForTenancy {Id = Guid.NewGuid(), Name = SecondTenant});
                        session.SaveChanges();
                    }
                }
            }
        }

        private static void AddMarten(IServiceCollection services, DummyTenancyContext tenancyContext)
        {
            services.AddSingleton(tenancyContext);
            services.AddSingleton<ITenancyPerSchemaStoreFactory, TenancyPerSchemaStoreFactory>();
            services.AddSingleton<Action<string, StoreOptions>>((tenantId, options) =>
            {
                options.DatabaseSchemaName = tenantId;
                options.Connection(Settings.ConnectionString);
            });

            // This can be overridden by the expression following
            services.AddScoped<ISessionFactory, TenancyPerSchemaSessionFactory>();

            services.AddScoped(s => s.GetRequiredService<ISessionFactory>().QuerySession());
            services.AddScoped(s => s.GetRequiredService<ISessionFactory>().OpenSession());
        }
    }
}
