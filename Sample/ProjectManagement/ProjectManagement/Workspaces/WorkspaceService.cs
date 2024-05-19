using ProjectManagement.Workspaces.Slug;

namespace ProjectManagement.Workspaces;

public static class WorkspaceService
{
    public record CreateWorkspace(
        Guid WorkspaceId,
        string Name,
        string TaskPrefix,
        Guid CreatedById
    );

    public static WorkspaceCreated Handle(CreateWorkspace command) =>
        new(
            WorkspaceId: command.WorkspaceId,
            Name: command.Name,
            TaskPrefix: command.TaskPrefix,
            Slug: SlugGenerator.New(command.Name),
            CreatedById: command.CreatedById
        );
}
