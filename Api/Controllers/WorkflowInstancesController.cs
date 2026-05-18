using System.Security.Claims;
using FlowGate.Core.Application.Contracts;
using FlowGate.Core.Application.Engine;
using FlowGate.Core.Application.Security;
using FlowGate.Core.Application.Services;
using FlowGate.Core.Domain.Enums;
using FlowGate.Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowGate.Core.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.WorkflowReaders)]
[Route("api/workflow-instances")]
public sealed class WorkflowInstancesController(
    IWorkflowService workflowService,
    IWorkflowEngine workflowEngine,
    FlowGateDbContext dbContext) : ControllerBase
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

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.StandardUser)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWorkflowInstanceRequest request, CancellationToken cancellationToken)
    {
        var requesterIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(requesterIdString, out var requesterId))
            return Unauthorized();

        var command = new CreateRequestCommand(request.WorkflowKey, request.Title, request.FormData, requesterId);
        var instanceId = await workflowEngine.StartAsync(command, cancellationToken);

        return Created($"/api/workflow-instances/{instanceId}", new { id = instanceId });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var instance = await dbContext.WorkflowInstances
            .AsNoTracking()
            .Include(i => i.Workflow)
            .Include(i => i.Requester)
            .Include(i => i.ApprovalSteps)
                .ThenInclude(s => s.AssignedUser)
            .Include(i => i.AuditLogs)
                .ThenInclude(l => l.PerformedByUser)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (instance is null)
            return NotFound();

        var dto = new WorkflowInstanceDetailDto(
            instance.Id,
            instance.WorkflowId,
            instance.Workflow.Key,
            instance.Workflow.Name,
            instance.Title,
            instance.CurrentStatus.ToString(),
            instance.RequesterId,
            instance.Requester.DisplayName,
            instance.FormDataJson is null ? null
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(instance.FormDataJson),
            instance.ApprovalSteps.OrderBy(s => s.StepIndex).Select(s => new ApprovalStepDetailDto(
                s.Id,
                s.StepIndex,
                s.GroupId,
                s.Name,
                s.RequiredRole,
                s.Status.ToString(),
                s.AssignedUser?.DisplayName,
                s.DecisionComment,
                s.DecidedAtUtc)).ToList(),
            instance.AuditLogs.OrderBy(l => l.CreatedAtUtc).Select(l => new AuditLogEntryDto(
                l.Id,
                l.Action,
                l.PerformedByUser?.DisplayName ?? l.CreatedBy,
                null,
                l.CreatedAtUtc)).ToList(),
            instance.CreatedAtUtc,
            instance.SubmittedAtUtc,
            instance.CompletedAtUtc);

        return Ok(dto);
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitAsync(Guid id, CancellationToken cancellationToken)
    {
        var performedBy = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        await workflowEngine.SubmitAsync(id, performedBy, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/decide")]
    [Authorize(Policy = AuthorizationPolicies.Approver)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecideAsync(Guid id, [FromBody] SubmitDecisionRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DecisionType>(request.Decision, ignoreCase: true, out var decisionType))
            return BadRequest(new { error = $"Invalid decision '{request.Decision}'. Valid values: Approve, Reject, RequestAdjustment." });

        var performedBy = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";

        var command = new SubmitDecisionCommand(id, request.StepId, decisionType, performedBy, request.Comment);
        await workflowEngine.ProcessDecisionAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var performedBy = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        await workflowEngine.CancelAsync(id, performedBy, cancellationToken);
        return NoContent();
    }

    [HttpGet("pending")]
    [Authorize(Policy = AuthorizationPolicies.Approver)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingAsync(CancellationToken cancellationToken)
    {
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToHashSet();

        var pendingSteps = await dbContext.ApprovalSteps
            .AsNoTracking()
            .Include(s => s.WorkflowInstance)
                .ThenInclude(i => i.Workflow)
            .Include(s => s.WorkflowInstance)
                .ThenInclude(i => i.Requester)
            .Where(s => s.Status == ApprovalStepStatus.InProgress &&
                        userRoles.Contains(s.RequiredRole))
            .OrderBy(s => s.WorkflowInstance.CreatedAtUtc)
            .Select(s => new PendingApprovalDto(
                s.WorkflowInstanceId,
                s.WorkflowInstance.Title,
                s.WorkflowInstance.Workflow.Name,
                s.WorkflowInstance.Requester.DisplayName,
                s.Id,
                s.Name,
                s.ExecutionType.ToString(),
                s.GroupId,
                s.WorkflowInstance.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(pendingSteps);
    }
}