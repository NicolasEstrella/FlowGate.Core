using FlowGate.Core.Application.Security;
using FlowGate.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowGate.Core.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.WorkflowReaders)]
[Route("api/workflows")]
public sealed class WorkflowsController(IWorkflowService workflowService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var workflows = await workflowService.GetWorkflowsAsync(cancellationToken);
        return Ok(workflows);
    }
}