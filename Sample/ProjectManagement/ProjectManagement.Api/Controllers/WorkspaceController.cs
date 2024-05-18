using ProjectManagement.Api.Core.Users;
using ProjectManagement.Workspaces;
using Marten;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Scenarios.WorkspaceCreation;

namespace ProjectManagement.Api.Controllers;

using static UserIdProvider;

[ApiController]
[Route("[controller]")]
public class WorkspaceController: ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WorkspaceDetails>> Post(
        [FromServices] IDocumentSession documentSession,
        CreateWorkspaceRequest request
    )
    {
        var workspaceId = await documentSession.CreateWorkspace(
            GetUserId(HttpContext),
            request.Name,
            request.TaskPrefix
        );

        return Created($"/workspace/{workspaceId}", workspaceId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkspaceDetails>> GetById([FromServices] IQuerySession querySession, Guid id)
    {
        var workspace = await querySession.LoadAsync<WorkspaceDetails>(id);

        return workspace is not null ? Ok(workspace) : NotFound();
    }
}

public record CreateWorkspaceRequest(
    string Name,
    string TaskPrefix
);
