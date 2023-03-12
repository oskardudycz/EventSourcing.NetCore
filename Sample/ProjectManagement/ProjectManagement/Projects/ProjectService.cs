namespace ProjectManagement.Projects;

public static class ProjectService
{
    public record CreateBacklogProject(
        Guid ProjectId,
        Guid WorkspaceId,
        Guid CreatedById
    );

    public static ProjectCreated Handle(CreateBacklogProject command) =>
        new ProjectCreated(
            ProjectId: command.ProjectId,
            Name: "Backlog",
            StartDate: null,
            EndDate: null,
            Status: ProjectStatus.Active,
            Slug: "backlog",
            WorkspaceId: command.WorkspaceId,
            CreatedById: command.CreatedById,
            IsBacklog: true
        );
}
