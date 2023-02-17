using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Core.Reflection;

public static class ObjectFactory<T>
{
    public static readonly Func<T> GetDefaultOrUninitialized = Creator();

    private static Func<T> Creator()
    {
        var t = typeof(T);
        if (t == typeof(string))
            return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();

        if (t.HasDefaultConstructor())
            return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();

        return () => (T)FormatterServices.GetUninitializedObject(t);
    }
}

public static class ObjectFactory
{
    public static bool HasDefaultConstructor(this Type t)
    {
        return t.IsValueType || t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null) != null;
    }
}
