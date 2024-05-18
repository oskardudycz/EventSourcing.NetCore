namespace ProjectManagement.Workspaces;

public record WorkspaceCreated(
    Guid WorkspaceId,
    string Name,
    string TaskPrefix,
    string Slug,
    Guid CreatedById
);
