namespace Core.Exceptions;

public class AggregateNotFoundException: Exception
{
    private AggregateNotFoundException(string typeName, string id): base($"{typeName} with id '{id}' was not found")
    {
    }

    public static AggregateNotFoundException For<T>(Guid id) =>
        For<T>(id.ToString());

    public static AggregateNotFoundException For<T>(string id) => new(typeof(T).Name, id);
}
