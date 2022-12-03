using Core.Ids;

namespace Carts.Tests.Stubs.Ids;

public class FakeIdGenerator: IIdGenerator
{
    public Guid? LastGeneratedId { get; private set; }
    public Guid New() => (LastGeneratedId = Guid.NewGuid()).Value;
}
