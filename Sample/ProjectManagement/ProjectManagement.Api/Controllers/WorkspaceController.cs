using ProjectManagement.Api.Core.Users;
using ProjectManagement.Core.Marten;
using ProjectManagement.Workspaces;
using Marten;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Projects;

namespace ProjectManagement.Api.Controllers;

using static WorkspaceService;
using static ProjectService;
using static UserIdProvider;

[ApiController]
[Route("[controller]")]
public class WorkspaceController: ControllerBase
{
    private readonly IDocumentSession documentSession;

    public WorkspaceController(IDocumentSession documentSession) =>
        this.documentSession = documentSession;

    [HttpPost]
    public async Task<ActionResult<WorkspaceDetails>> Post(CreateWorkspaceRequest request)
    {
        var userId = GetUserId(HttpContext);
        var workspaceId = MartenIdGenerator.New();
        var backlogId = MartenIdGenerator.New();

        await documentSession.ComposeAsync(
            (workspaceId, Handle(new CreateWorkspace(workspaceId, request.Name, request.TaskPrefix, userId))),
            (backlogId, Handle(new CreateBacklogProject(backlogId, workspaceId, userId)))
        );

        var workspace = await documentSession.LoadAsync<WorkspaceDetails>(workspaceId);

        if (workspace is null)
            return NotFound();

        return Ok(workspace);
    }
}

public record CreateWorkspaceRequest(
    string Name,
    string TaskPrefix
);
