using FlowGate.Core.Application.Contracts;

namespace FlowGate.Core.Application.Services;

public interface IWorkflowService
{
    Task<IReadOnlyCollection<WorkflowSummaryDto>> GetWorkflowsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WorkflowInstanceSummaryDto>> GetWorkflowInstancesAsync(CancellationToken cancellationToken);
}