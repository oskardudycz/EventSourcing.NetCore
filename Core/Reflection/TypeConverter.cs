namespace Core.Reflection;

public static class ValueTypeConverter
{
    public static T ChangeType<T>(object value) =>
        (T)ChangeType(typeof(T), value)!;

    public static object? ChangeType(Type t, object value)
    {
        var tc = TypeDescriptor.GetConverter(t);
        return tc.ConvertFrom(value);
    }

    public static void RegisterTypeConverter<T, TC>() where TC : TypeConverter =>
        TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
}
