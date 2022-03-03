using System.Collections.Concurrent;

namespace Core.ElasticSearch.Indices;

public class IndexNameMapper
{
    private static readonly IndexNameMapper Instance = new();

    private readonly ConcurrentDictionary<Type, string> typeNameMap = new();

    public static void AddCustomMap<TStream>(string mappedStreamName) =>
        AddCustomMap(typeof(TStream), mappedStreamName);

    public static void AddCustomMap(Type streamType, string mappedStreamName)
    {
        Instance.typeNameMap.AddOrUpdate(streamType, mappedStreamName, (_, _) => mappedStreamName);
    }

    public static string ToIndexPrefix<TStream>() => ToIndexPrefix(typeof(TStream));

    public static string ToIndexPrefix(Type streamType) => Instance.typeNameMap.GetOrAdd(streamType, _ =>
    {
        var modulePrefix = streamType.Namespace!.Split(".").First();
        return $"{modulePrefix}-{streamType.Name}".ToLower();
    });

    public static string ToIndexName<TStream>(object? tenantId = null) =>
        ToIndexName(typeof(TStream));

    public static string ToIndexName(Type streamType, object? tenantId = null)
    {
        var tenantPrefix = tenantId != null ? $"{tenantId}-"  : "";

        return $"{tenantPrefix}{ToIndexPrefix(streamType)}".ToLower();
    }

}