using FlowGate.Core.Domain.Entities;
using FlowGate.Core.Domain.Enums;

namespace FlowGate.Core.Application.Engine;

public interface IWorkflowStateMachine
{
    void TransitionTo(WorkflowInstance instance, WorkflowInstanceStatus targetStatus, string performedBy, string? comment = null);
}
