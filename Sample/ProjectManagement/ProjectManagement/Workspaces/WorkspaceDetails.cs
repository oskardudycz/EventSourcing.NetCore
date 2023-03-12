namespace ProjectManagement.Workspaces;

public record WorkspaceDetails(
    Guid Id,
    string Name,
    string TaskPrefix,
    string Slug,
    ProjectInfo[] Projects,
    Guid CreatedById
);

public record ProjectInfo(
    Guid Id,
    string Name
);
