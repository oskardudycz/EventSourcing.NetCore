using ProjectManagement.Core.Marten;
using ProjectManagement.Projects;
using ProjectManagement.Workspaces.Slug;

namespace ProjectManagement.Workspaces;

public static class WorkspaceService
{
    public static (WorkspaceCreated, ProjectCreated) Handle(CreateWorkspace command)
    {
        var slug = SlugGenerator.New(command.Name);
        var backlogId = MartenIdGenerator.New();

        return new(
            new WorkspaceCreated(
                WorkspaceId: command.WorkspaceId,
                Name: command.Name,
                TaskPrefix: command.TaskPrefix,
                Slug: slug,
                CreatedById: command.CreatedById
            ),
            new ProjectCreated(
                ProjectId: backlogId,
                Name: "Backlog",
                StartDate: null,
                EndDate: null,
                Status: ProjectStatus.Active,
                Slug: "backlog",
                WorkspaceId: command.WorkspaceId,
                CreatedById: command.CreatedById,
                IsBacklog: true
            )
        );
    }
}

public record CreateWorkspace(
    Guid WorkspaceId,
    string Name,
    string TaskPrefix,
    Guid CreatedById
);
