using ProjectManagement.Projects;
using ProjectManagement.Workspaces;

namespace ProjectManagement.Scenarios.WorkspaceCreation;

using static WorkspaceService;
using static ProjectService;

public static class WorkspaceCreationScenario
{
    public static (WorkspaceCreated, ProjectCreated) CreateWorkspace(
        Func<Guid> generateId,
        Guid userId,
        string name,
        string taskPrefix
    )
    {
        var workspaceId = generateId();
        var backlogId = generateId();

        return new(
            Handle(new CreateWorkspace(workspaceId, name, taskPrefix, userId)),
            Handle(new CreateBacklogProject(backlogId, workspaceId, userId))
        );
    }
}
