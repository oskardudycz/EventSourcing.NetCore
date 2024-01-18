using Marten.Schema.Identity;

namespace ProjectManagement.Core.Marten;

public static class MartenIdGenerator
{
    public static Guid New() => CombGuidIdGeneration.NewGuid();
}
