using Marten;
using ProjectManagement.Core.Marten;

namespace ProjectManagement.Scenarios.WorkspaceCreation;

public static class WorkspaceCreationScenarioHandler
{
    public static async Task<Guid> CreateWorkspace(
        this IDocumentSession documentSession,
        Guid userId,
        string name,
        string taskPrefix
    )
    {
        var (workspaceCreated, projectCreated) =
            WorkspaceCreationScenario.CreateWorkspace(MartenIdGenerator.New, userId, name, taskPrefix);

        await documentSession.ComposeAsync(
            (workspaceCreated.WorkspaceId, workspaceCreated),
            (projectCreated.ProjectId, projectCreated)
        );

        return projectCreated.WorkspaceId;
    }
}
