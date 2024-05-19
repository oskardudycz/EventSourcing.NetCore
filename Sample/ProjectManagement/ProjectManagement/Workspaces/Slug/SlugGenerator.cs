namespace ProjectManagement.Workspaces.Slug;

public static class SlugGenerator
{
    public static string New(string name) =>
        $"{name}-{Guid.NewGuid().ToString("N")[..5]}";
}
