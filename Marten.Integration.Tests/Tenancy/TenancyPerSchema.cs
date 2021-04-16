using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marten.Integration.Tests.Tenancy
{
    public interface IDocumentStoreFactory<in T> : IDisposable
    {
        IDocumentStore Get(T context);
    }

    public abstract class DocumentStoreFactory<T> : IDocumentStoreFactory<T>
    {
        private readonly ConcurrentDictionary<string, DocumentStore> _stores = new ConcurrentDictionary<string, DocumentStore>();

        public IDocumentStore Get(T context)
        {
            var storeKey = GetKey(context);
            return _stores.GetOrAdd(storeKey, (key) =>
            {
                var storeOptions = new StoreOptions();
                Configure(context, storeOptions);
                return new DocumentStore(storeOptions);
            });
        }

        public void Dispose()
        {
            foreach (var documentStore in _stores.Values)
            {
                documentStore.Dispose();
            }
        }

        protected abstract void Configure(T initialization, StoreOptions storeOptions);
        protected abstract string GetKey(T initialization);

    }

    // expression configuration example.
    public class DelegateDocumentStoreFactory<T> : DocumentStoreFactory<T> where T : class
    {
        private readonly Action<T, StoreOptions> _configure;
        private readonly Func<T, string> _documentStoreKeyExpression;

        public DelegateDocumentStoreFactory(
            Expression<Func<T, string>> documentStoreKeyExpression,
            Action<T, StoreOptions> configure
            )
        {
            _configure = configure;
            _documentStoreKeyExpression = documentStoreKeyExpression.Compile();
        }

        protected override void Configure(T initialization, StoreOptions storeOptions) => _configure(initialization, storeOptions);


        protected override string GetKey(T initialization) => _documentStoreKeyExpression(initialization);
    }

    // concrete example
    internal class SampleDocumentStoreFactory : DocumentStoreFactory<DummyTenancyContext>
    {
        protected override void Configure(DummyTenancyContext initialization, StoreOptions storeOptions)
        {
            storeOptions.DatabaseSchemaName = initialization.TenantId;
            storeOptions.Connection(initialization.ConnectionString);

        }
        protected override string GetKey(DummyTenancyContext initialization) => initialization.TenantId;
    }

    public class DummyTenancyContext
    {
        // this should be normally taken from the http context or other

        // this can be as complex/simple as it needs to be, for all intents and purposes it's just a class maintained by the application at an expected lifetime scope.
        public string TenantId { get; set; }
        public string ConnectionString { get; set; }
       
    }

    public class TenancyPerSchemaSessionFactory : ISessionFactory
    {
        private readonly IDocumentStoreFactory<DummyTenancyContext> storeFactory;
        private readonly DummyTenancyContext tenancyContext;

        public TenancyPerSchemaSessionFactory(IDocumentStoreFactory<DummyTenancyContext> storeFactory, DummyTenancyContext tenancyContext)
        {
            this.storeFactory = storeFactory;
            this.tenancyContext = tenancyContext;
        }

        public IQuerySession QuerySession()
        {
            return storeFactory.Get(tenancyContext).QuerySession();
        }

        public IDocumentSession OpenSession()
        {
            return storeFactory.Get(tenancyContext).LightweightSession(IsolationLevel.Serializable);
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

            AddMarten(services);

            using (var sp = services.BuildServiceProvider())
            {
                // simulate scope per HTTP request with different tenant
                using (var firstScope = sp.CreateScope())
                {
                    
                    firstScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().TenantId = FirstTenant;
                    firstScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().ConnectionString = Settings.ConnectionString;

                    using (var session = firstScope.ServiceProvider.GetRequiredService<IDocumentSession>())
                    {
                        session.Insert(new TestDocumentForTenancy { Id = Guid.NewGuid(), Name = FirstTenant });
                        session.SaveChanges();
                    }
                }

                // simulate scope per HTTP request with different tenant
                using (var secondScope = sp.CreateScope())
                {
                    secondScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().TenantId = SecondTenant;
                    secondScope.ServiceProvider.GetRequiredService<DummyTenancyContext>().ConnectionString = Settings.ConnectionString; // in my scenario these connection strings are different.

                    using (var session = secondScope.ServiceProvider.GetRequiredService<IDocumentSession>())
                    {
                        session.Insert(new TestDocumentForTenancy { Id = Guid.NewGuid(), Name = SecondTenant });
                        session.SaveChanges();
                    }
                }
            }
        }

        private static void AddMarten(IServiceCollection services)
        {
            services.AddScoped<DummyTenancyContext>();

            // register a concrete
            services.UseDocumentStoreFactory<SampleDocumentStoreFactory, DummyTenancyContext>();
            // or use expression
            services.UseDocumentStoreFactory<DummyTenancyContext>(context => context.TenantId, (context, options) =>
            {
                options.DatabaseSchemaName = context.TenantId;
                options.Connection(context.ConnectionString);
            });
            
            // This can be overridden by the expression following
            services.AddScoped<ISessionFactory, TenancyPerSchemaSessionFactory>();

            services.AddScoped(s => s.GetRequiredService<ISessionFactory>().QuerySession());
            services.AddScoped(s => s.GetRequiredService<ISessionFactory>().OpenSession());
        }

    }

    public static class MoveToMartenExtensions
    {
        public static IServiceCollection UseDocumentStoreFactory<TImplementation, TConfigurationElement>(this IServiceCollection services)
            where TImplementation : class, IDocumentStoreFactory<TConfigurationElement>
        {
            // change return type to whatever marten needs
            services.AddSingleton<IDocumentStoreFactory<TConfigurationElement>, TImplementation>();
            return services;
        }

        public static IServiceCollection UseDocumentStoreFactory<T>(this IServiceCollection services,
            Expression<Func<T, string>> documentStoreKeyExpression,
            Action<T, StoreOptions> configure)
            where T : class
        {
            // change return type to whatever marten needs
            var delegateFactory = new DelegateDocumentStoreFactory<T>(documentStoreKeyExpression, configure);
            services.AddSingleton<IDocumentStoreFactory<T>>(delegateFactory);
            return services;
        }
    }
}
