namespace Tickets.Maintenance.Commands;

public record RebuildProjection(
    string ViewName
)
{
    public static RebuildProjection Create(string? viewName)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return new RebuildProjection(viewName);
    }
}
