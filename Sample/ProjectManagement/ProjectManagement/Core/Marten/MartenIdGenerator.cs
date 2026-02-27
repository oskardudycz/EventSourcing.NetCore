namespace ProjectManagement.Core.Marten;

public static class MartenIdGenerator
{
    public static Guid New() => Guid.CreateVersion7();
}
