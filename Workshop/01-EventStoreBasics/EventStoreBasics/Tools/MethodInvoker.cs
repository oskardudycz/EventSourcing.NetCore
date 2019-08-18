using System.Linq;
using System.Reflection;

namespace EventStoreBasics.Tools
{
    public static class MethodInvoker
    {
        public static object InvokeIfExists<T>(this T item, string methodName, object param)
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

            return method?.Invoke(item, new [] { param });
        }

        public static void Set<T>(this T item, string propertyName, object value)
        {
            item.GetType()
                .GetProperty(propertyName)
                .SetValue(item, value);
        }

    }
}
