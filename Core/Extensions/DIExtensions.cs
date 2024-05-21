using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions;

public static class DIExtensions
{
    // Taken from: https://stackoverflow.com/a/77559901
    public static IServiceCollection AllowResolvingKeyedServicesAsDictionary(
        this IServiceCollection sc)
    {
        // KeyedServiceCache caches all the keys of a given type for a
        // specific service type. By making it a singleton we only have
        // determine the keys once, which makes resolving the dict very fast.
        sc.AddSingleton(typeof(KeyedServiceCache<,>));

        // KeyedServiceCache depends on the IServiceCollection to get
        // the list of keys. That's why we register that here as well, as it
        // is not registered by default in MS.DI.
        sc.AddSingleton(sc);

        // Last we make the registration for the dictionary itself, which maps
        // to our custom type below. This registration must be  transient, as
        // the containing services could have any lifetime and this registration
        // should by itself not cause Captive Dependencies.
        sc.AddTransient(typeof(IDictionary<,>), typeof(KeyedServiceDictionary<,>));

        // For completeness, let's also allow IReadOnlyDictionary to be resolved.
        sc.AddTransient(
            typeof(IReadOnlyDictionary<,>), typeof(KeyedServiceDictionary<,>));

        return sc;
    }

    // We inherit from ReadOnlyDictionary, to disallow consumers from changing
    // the wrapped dependencies while reusing all its functionality. This way
    // we don't have to implement IDictionary<T,V> ourselves; too much work.
    private sealed class KeyedServiceDictionary<TKey, TService>(
        KeyedServiceCache<TKey, TService> keys, IServiceProvider provider)
        : ReadOnlyDictionary<TKey, TService>(Create(keys, provider))
        where TKey : notnull
        where TService : notnull
    {
        private static Dictionary<TKey, TService> Create(
            KeyedServiceCache<TKey, TService> keys, IServiceProvider provider)
        {
            var dict = new Dictionary<TKey, TService>(capacity: keys.Keys.Length);

            foreach (TKey key in keys.Keys)
            {
                dict[key] = provider.GetRequiredKeyedService<TService>(key);
            }

            return dict;
        }
    }

    private sealed class KeyedServiceCache<TKey, TService>(IServiceCollection sc)
        where TKey : notnull
        where TService : notnull
    {
        // Once this class is resolved, all registrations are guaranteed to be
        // made, so we can, at that point, safely iterate the collection to get
        // the keys for the service type.
        public TKey[] Keys { get; } = (
            from service in sc
            where service.ServiceKey != null
            where service.ServiceKey!.GetType() == typeof(TKey)
            where service.ServiceType == typeof(TService)
            select (TKey)service.ServiceKey!)
            .ToArray();
    }
}
