using FlowGate.Core.Application.Engine;
using FlowGate.Core.Domain.Entities;
using FlowGate.Core.Domain.Enums;

namespace FlowGate.Core.Infrastructure.Engine;

public sealed class WorkflowStateMachineService : IWorkflowStateMachine
{
    private static readonly Dictionary<WorkflowInstanceStatus, HashSet<WorkflowInstanceStatus>> AllowedTransitions = new()
    {
        [WorkflowInstanceStatus.Draft] = [WorkflowInstanceStatus.Submitted, WorkflowInstanceStatus.Cancelled],
        [WorkflowInstanceStatus.Submitted] = [WorkflowInstanceStatus.InApproval, WorkflowInstanceStatus.Cancelled],
        [WorkflowInstanceStatus.InApproval] =
        [
            WorkflowInstanceStatus.Approved,
            WorkflowInstanceStatus.Rejected,
            WorkflowInstanceStatus.AdjustmentsRequested,
            WorkflowInstanceStatus.Cancelled
        ],
        [WorkflowInstanceStatus.AdjustmentsRequested] = [WorkflowInstanceStatus.Submitted, WorkflowInstanceStatus.Cancelled],
        [WorkflowInstanceStatus.Approved] = [],
        [WorkflowInstanceStatus.Rejected] = [],
        [WorkflowInstanceStatus.Cancelled] = []
    };

    public void TransitionTo(WorkflowInstance instance, WorkflowInstanceStatus targetStatus, string performedBy, string? comment = null)
    {
        if (!AllowedTransitions.TryGetValue(instance.CurrentStatus, out var allowed) || !allowed.Contains(targetStatus))
        {
            throw new InvalidOperationException(
                $"Transition from '{instance.CurrentStatus}' to '{targetStatus}' is not allowed.");
        }

        instance.CurrentStatus = targetStatus;
        instance.ModifiedBy = performedBy;
        instance.ModifiedAtUtc = DateTimeOffset.UtcNow;
    }
}
