using FlowGate.Core.Application.Contracts;
using FlowGate.Core.Application.Services;
using FlowGate.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowGate.Core.Infrastructure.Services;

public sealed class WorkflowService(FlowGateDbContext dbContext) : IWorkflowService
{
    public async Task<IReadOnlyCollection<WorkflowSummaryDto>> GetWorkflowsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Workflows
            .AsNoTracking()
            .OrderBy(workflow => workflow.Name)
            .Select(workflow => new WorkflowSummaryDto(
                workflow.Id,
                workflow.Key,
                workflow.Name,
                workflow.Version,
                workflow.IsActive,
                dbContext.WorkflowInstances.Count(instance => instance.WorkflowId == workflow.Id)))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WorkflowInstanceSummaryDto>> GetWorkflowInstancesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowInstances
            .AsNoTracking()
            .Include(instance => instance.Workflow)
            .Include(instance => instance.Requester)
            .OrderByDescending(instance => instance.CreatedAtUtc)
            .Select(instance => new WorkflowInstanceSummaryDto(
                instance.Id,
                instance.WorkflowId,
                instance.Workflow.Name,
                instance.Title,
                instance.CurrentStatus.ToString(),
                instance.RequesterId,
                instance.Requester.DisplayName,
                instance.CreatedAtUtc,
                instance.SubmittedAtUtc,
                instance.CompletedAtUtc))
            .ToArrayAsync(cancellationToken);
    }
}