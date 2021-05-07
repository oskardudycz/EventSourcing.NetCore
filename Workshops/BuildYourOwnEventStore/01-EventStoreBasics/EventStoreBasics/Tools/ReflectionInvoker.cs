using System.Linq;
using System.Reflection;

namespace EventStoreBasics.Tools
{
    public static class ReflectionInvoker
    {
        public static void InvokeIfExists<T>(this T item, string methodName, object param) where T : notnull
        {
            var method = item.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m =>
                        {
                            var parameters = m.GetParameters();
                            return
                                m.Name == methodName
                                && parameters.Length == 1
                                && parameters.Single().ParameterType == param.GetType();
                        })
                    .SingleOrDefault();

            method?.Invoke(item, new [] { param });
        }

        public static void SetIfExists<T>(this T item, string propertyName, object value) where T : notnull
        {
            item.GetType()
                .GetProperty(propertyName)?
                .SetValue(item, value);
        }

    }
}
