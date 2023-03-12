using ProjectManagement.Projects;

namespace ProjectManagement.Workspaces;

public record WorkspaceCreated(
    Guid WorkspaceId,
    string Name,
    string TaskPrefix,
    string Slug,
    Guid CreatedById
);

public record ProjectCreated(
    Guid ProjectId,
    string Name,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    ProjectStatus Status,
    string Slug,
    Guid WorkspaceId,
    Guid CreatedById,
    bool IsBacklog
);
