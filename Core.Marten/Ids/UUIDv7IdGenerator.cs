using Core.Ids;

namespace Core.Marten.Ids;

public class UUIDv7IdGenerator: IIdGenerator
{
    public Guid New() => Guid.CreateVersion7();
}
