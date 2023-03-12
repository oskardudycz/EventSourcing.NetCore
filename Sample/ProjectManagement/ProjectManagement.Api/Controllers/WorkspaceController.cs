using ProjectManagement.Api.Core.Users;
using ProjectManagement.Core.Marten;
using ProjectManagement.Workspaces;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace ProjectManagement.Api.Controllers;

using static WorkspaceService;
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
        var cmd = new CreateWorkspace(workspaceId, request.Name, request.TaskPrefix, userId);

        var (workspaceCreatedEvent, backlogCreatedEvent) = Handle(cmd);

        documentSession.Events.Append(workspaceId, workspaceCreatedEvent);
        documentSession.Events.Append(backlogCreatedEvent.ProjectId, backlogCreatedEvent);
        await documentSession.SaveChangesAsync();

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
