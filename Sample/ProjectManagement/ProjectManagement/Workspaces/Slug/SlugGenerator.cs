namespace ProjectManagement.Workspaces.Slug;

public static class SlugGenerator
{
    public static string New(string name) =>
        $"{name}-{Guid.CreateVersion7().ToString("N")[..5]}";
}
