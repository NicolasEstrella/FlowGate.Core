using FlowGate.Core.Domain.Entities;

namespace FlowGate.Core.Application.Engine;

public interface IWorkflowStore
{
    Task<Domain.Entities.Workflow?> FindWorkflowByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> FindInstanceAsync(Guid id, CancellationToken cancellationToken = default);
    void AddInstance(WorkflowInstance instance);
    void AddStep(ApprovalStep step);
    void AddAuditLog(AuditLog log);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
