namespace ProjectManagement.Projects;

public enum ProjectStatus
{
    Active,
    Inactive,
    Completed,
    Cancelled
}

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
