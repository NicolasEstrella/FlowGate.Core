using FlowGate.Core.Application.Security;
using FlowGate.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowGate.Core.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.WorkflowReaders)]
[Route("api/workflow-instances")]
public sealed class WorkflowInstancesController(IWorkflowService workflowService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var workflowInstances = await workflowService.GetWorkflowInstancesAsync(cancellationToken);
        return Ok(workflowInstances);
    }
}