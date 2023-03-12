using ProjectManagement.Api.Core.Users;
using ProjectManagement.Core.Marten;
using ProjectManagement.Workspaces;
using Marten;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Projects;
using ProjectManagement.Scenarios.WorkspaceCreation;

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
        var workspaceId = await documentSession.CreateWorkspace(
            GetUserId(HttpContext),
            request.Name,
            request.TaskPrefix
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
