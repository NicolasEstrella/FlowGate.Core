using FlowGate.Core.Application.Engine;
using FlowGate.Core.Domain.Entities;

namespace FlowGate.Core.Tests.Unit.Engine;

public sealed class FakeWorkflowEngineContext : IWorkflowStore
{
    private readonly List<Workflow> _workflows = [];
    private readonly List<User> _users = [];

    public List<WorkflowInstance> WorkflowInstances { get; } = [];
    public List<ApprovalStep> ApprovalSteps { get; } = [];
    public List<AuditLog> AuditLogs { get; } = [];

    public void AddWorkflow(Workflow workflow) => _workflows.Add(workflow);
    public void AddUser(User user) => _users.Add(user);

    public Task<Workflow?> FindWorkflowByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var workflow = _workflows.FirstOrDefault(w => w.Key == key && w.IsActive);
        return Task.FromResult<Workflow?>(workflow);
    }

    public Task<WorkflowInstance?> FindInstanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instance = WorkflowInstances.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(instance);
    }

    public void AddInstance(WorkflowInstance instance) => WorkflowInstances.Add(instance);

    public void AddStep(ApprovalStep step) => ApprovalSteps.Add(step);

    public void AddAuditLog(AuditLog log) => AuditLogs.Add(log);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
