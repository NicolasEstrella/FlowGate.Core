using FlowGate.Core.Application.Engine;
using FlowGate.Core.Domain.Entities;
using FlowGate.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowGate.Core.Infrastructure.Engine;

public sealed class EfWorkflowStore(FlowGateDbContext dbContext) : IWorkflowStore
{
    public async Task<Workflow?> FindWorkflowByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await dbContext.Workflows
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Key == key && w.IsActive, cancellationToken);
    }

    public async Task<WorkflowInstance?> FindInstanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkflowInstances
            .Include(i => i.ApprovalSteps)
            .Include(i => i.AuditLogs)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public void AddInstance(WorkflowInstance instance) => dbContext.WorkflowInstances.Add(instance);

    public void AddStep(ApprovalStep step) => dbContext.ApprovalSteps.Add(step);

    public void AddAuditLog(AuditLog log) => dbContext.AuditLogs.Add(log);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
